// Original source: Crass
// Modified by: detached64
// Date: 2025/02/15

#include "bits.h"
#include <stdio.h>
#include <stdlib.h>

typedef struct huffman_node
{
	uint weight;
	byte ascii;
	uint code;
	uint code_lengths;
	struct huffman_node* parent;
	struct huffman_node* left_child;
	struct huffman_node* right_child;
}huffman_node_t;

int huffman_tree_create_dec(struct bits* bits, ushort children[2][255], unsigned int* index, ushort* retval)
{
	unsigned int bitval;
	ushort child;

	if (bit_get_high(bits, &bitval))
		return -1;

	if (bitval)
	{
		unsigned int parent;

		parent = *index;
		*index = parent + 1;

		if (huffman_tree_create_dec(bits, children, index, &child))
			return -1;
		children[0][parent - 256] = child;

		if (huffman_tree_create_dec(bits, children, index, &child))
			return -1;
		children[1][parent - 256] = child;

		child = parent;
	}
	else
	{
		unsigned int byteval;

		if (bits_get_high(bits, 8, &byteval))
			return -1;

		child = byteval;
	}
	*retval = child;

	return 0;
}

__declspec(dllexport) int huffman_uncompress(unsigned char* uncompr, unsigned long uncomprlen, unsigned char* compr, unsigned long comprlen)
{
	struct bits bits;
	ushort children[2][255];
	unsigned int index = 256;
	unsigned long max_uncomprlen;
	unsigned long act_uncomprlen;
	unsigned int bitval;
	ushort retval;

	bits_init(&bits, compr, comprlen);
	if (huffman_tree_create_dec(&bits, children, &index, &retval))
		return -1;
	if (retval != 256)
		return -1;

	index = 0;
	act_uncomprlen = 0;
	max_uncomprlen = uncomprlen;
	while (!bit_get_high(&bits, &bitval))
	{
		if (bitval)
			retval = children[1][index];
		else
			retval = children[0][index];

		if (retval >= 256)
			index = retval - 256;
		else
		{
			if (act_uncomprlen >= max_uncomprlen)
				break;
			uncompr[act_uncomprlen++] = (byte)retval;
			index = 0;
		}
	}
	return 0;
}

void huffman1_node_encode(huffman_node_t* node, unsigned int code, unsigned int code_lengths)
{
	if (node->left_child)
	{
		code <<= 1;
		code_lengths++;
		huffman1_node_encode(node->left_child, code, code_lengths);
		code |= 1;
		huffman1_node_encode(node->right_child, code, code_lengths);
	}
	else
	{
		node->code = code;
		node->code_lengths = code_lengths;
	}
}

int huffman_code_tree_encode(struct bits* bits, huffman_node_t* parent)
{
	if (parent->left_child)
	{
		if (bit_put_high(bits, 1))
			return -1;

		if (huffman_code_tree_encode(bits, parent->left_child))
			return -1;

		if (huffman_code_tree_encode(bits, parent->right_child))
			return -1;
	}
	else
	{
		if (bit_put_high(bits, 0))
			return -1;

		if (bits_put_high(bits, 8, (unsigned char*)&parent->ascii))
			return -1;
	}
	return 0;
}

huffman_node_t* huffman_child_init(huffman_node_t* child_node, unsigned int is_right_child)
{
	return child_node;
}

unsigned int huffman_tree_create_enc(huffman_node_t* nodes)
{
	huffman_node_t* pnodes[256], * pnode = 0;
	int leaves_node;
	int parent_node;
	int child_node;
	int i;

	for (i = 0; nodes[i].weight && i < 256; i++)
		pnodes[i] = &nodes[i];

	leaves_node = i;

	if (leaves_node < 2)
	{
		printf("Error: only one node in huffman tree\n");
		return -1;
	}

	parent_node = leaves_node;
	child_node = parent_node - 1;
	while (child_node > 0)
	{
		pnode = &nodes[parent_node++];
		pnode->left_child = huffman_child_init(pnodes[child_node--], 0);
		pnode->right_child = huffman_child_init(pnodes[child_node--], 1);
		pnode->left_child->parent = pnode->right_child->parent = pnode;
		pnode->weight = pnode->left_child->weight + pnode->right_child->weight;
		for (i = child_node; i >= 0; i--)
		{
			if (pnodes[i]->weight >= pnode->weight)
				break;
		}
		memmove(pnodes + i + 2, pnodes + i + 1, (child_node - i) * sizeof(huffman_node_t*));
		pnodes[i + 1] = pnode;
		child_node++;
	}
	huffman1_node_encode(pnode, 0, 0);

	return leaves_node;
}

int huffman_weight_compare(const void* node1, const void* node2)
{
	huffman_node_t* nodes[2] = { (huffman_node_t*)node1, (huffman_node_t*)node2 };

	return (int)nodes[1]->weight - (int)nodes[0]->weight;
}

int huffman_ascii_compare(const void* node1, const void* node2)
{
	huffman_node_t* nodes[2] = { (huffman_node_t*)node1, (huffman_node_t*)node2 };

	return (int)nodes[0]->ascii - (int)nodes[1]->ascii;
}

__declspec(dllexport) int huffman_compress(unsigned char* compr, unsigned long comprlen, unsigned char* uncompr, unsigned long uncomprlen)
{
	huffman_node_t nodes[2 * 256 - 1];
	unsigned int leaves;
	unsigned int output_bits;
	unsigned long i;
	huffman_node_t* root;
	struct bits bits;

	memset(nodes, 0, sizeof(nodes));

	for (i = 0; i < 256; i++)
		nodes[i].ascii = (byte)i;

	for (i = 0; i < uncomprlen; i++)
		nodes[uncompr[i]].weight++;

	qsort(nodes, 256, sizeof(huffman_node_t), huffman_weight_compare);

	leaves = huffman_tree_create_enc(nodes);

	root = &nodes[0];
	while (root->parent)
		root = root->parent;

	bits_init(&bits, compr, comprlen);
	if (huffman_code_tree_encode(&bits, root))
		return -1;

	qsort(nodes, 256, sizeof(huffman_node_t), huffman_ascii_compare);

	output_bits = bits.curbyte * 8 + bits.curbits;
	for (i = 0; i < uncomprlen; i++)
	{
		if (bits_put_high(&bits, nodes[uncompr[i]].code_lengths, (unsigned char*)&nodes[uncompr[i]].code))
			break;
		output_bits += nodes[uncompr[i]].code_lengths;
	}
	if (i != uncomprlen)
		return -1;
	bits_flush(&bits);

	return 0;
}