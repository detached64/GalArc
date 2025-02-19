// Original source: Crass
// Modified by: detached64
// Date: 2025/02/15

#include "def.h"
#include <stdio.h>
#include <string.h>
#include <stdlib.h>

struct bits
{
	unsigned long curbits;
	unsigned long curbyte;
	unsigned char cache;
	unsigned char* stream;
	unsigned long stream_length;
};

typedef struct huffman_node
{
	uint weight;
	byte ascii;
	uint code;			/* 哈夫曼编码值 */
	uint code_lengths;	/* 哈夫曼编码值的位数 */
	struct huffman_node* parent;
	struct huffman_node* left_child;
	struct huffman_node* right_child;
}huffman_node_t;

void bits_init(struct bits* bits, unsigned char* stream, unsigned long stream_length)
{
	memset(bits, 0, sizeof(*bits));
	bits->stream = stream;
	bits->stream_length = stream_length;
}

int bits_get_high(struct bits* bits, unsigned int req_bits, unsigned int* retval)
{
	unsigned int bits_value = 0;

	while (req_bits > 0)
	{
		unsigned int req;

		if (!bits->curbits)
		{
			if (bits->curbyte >= bits->stream_length)
				return -1;
			bits->cache = bits->stream[bits->curbyte++];
			bits->curbits = 8;
		}

		if (bits->curbits < req_bits)
			req = bits->curbits;
		else
			req = req_bits;

		bits_value <<= req;
		bits_value |= bits->cache >> (bits->curbits - req);
		bits->cache &= (1 << (bits->curbits - req)) - 1;
		req_bits -= req;
		bits->curbits -= req;
	}
	*retval = bits_value;
	return 0;
}

int bit_get_high(struct bits* bits, void* retval)
{
	return bits_get_high(bits, 1, (unsigned int*)retval);
}

int bit_put_high(struct bits* bits, unsigned char setval)
{
	bits->curbits++;
	bits->cache |= (setval & 1) << (8 - bits->curbits);
	if (bits->curbits == 8)
	{
		if (bits->curbyte >= bits->stream_length)
			return -1;
		bits->stream[bits->curbyte++] = bits->cache;
		bits->curbits = 0;
		bits->cache = 0;
	}
	return 0;
}

int bits_put_high(struct bits* bits, unsigned int req_bits, void* setval)
{
	unsigned int this_bits;
	unsigned int this_byte;
	unsigned int i;

	this_byte = req_bits / 8;
	this_bits = req_bits & 7;
	for (int k = (int)this_bits - 1; k >= 0; k--)
	{
		unsigned char bitval;

		bitval = !!(((unsigned char*)setval)[this_byte] & (1 << k));
		if (bit_put_high(bits, bitval))
			return -1;
	}
	this_bits = req_bits & ~7;
	this_byte--;
	for (i = 0; i < this_bits; i++)
	{
		unsigned char bitval;

		bitval = !!(((unsigned char*)setval)[this_byte - i / 8] & (1 << (7 - (i & 7))));
		if (bit_put_high(bits, bitval))
			return -1;
	}

	return 0;
}

void bits_flush(struct bits* bits)
{
	bits->stream[bits->curbyte] = bits->cache;
}

int huffman_tree_create(struct bits* bits, ushort children[2][255], unsigned int* index, ushort* retval)
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

		if (huffman_tree_create(bits, children, index, &child))
			return -1;
		children[0][parent - 256] = child;

		if (huffman_tree_create(bits, children, index, &child))
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

extern "C" NATIVE_API int huffman_uncompress(unsigned char* uncompr, unsigned long uncomprlen, unsigned char* compr, unsigned long comprlen)
{
	struct bits bits;
	ushort children[2][255];
	unsigned int index = 256;
	unsigned long max_uncomprlen;
	unsigned long act_uncomprlen;
	unsigned int bitval;
	ushort retval;

	bits_init(&bits, compr, comprlen);
	if (huffman_tree_create(&bits, children, &index, &retval))
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

unsigned int huffman_tree_create(huffman_node_t* nodes)
{
	huffman_node_t* pnodes[256], * pnode = 0;
	int leaves_node;				/* 有效的叶结点计数 */
	int parent_node;				/* 合并时新结点的位置索引 */
	int child_node;					/* 叶结点位置计数 */
	int i;

	/* 将出现过的(权值不为0的)叶节点放入队列 */
	for (i = 0; nodes[i].weight && i < 256; i++)
		pnodes[i] = &nodes[i];

	leaves_node = i;

	if (leaves_node < 2)
	{
		printf("有效的叶结点数目过少\n");
		return -1;
	}

	parent_node = leaves_node;
	child_node = parent_node - 1;
	while (child_node > 0)
	{
		pnode = &nodes[parent_node++];	/* 合并左右叶结点以后的新结点 */
		/* CUSTOM!! */
		pnode->left_child = huffman_child_init(pnodes[child_node--], 0);	/* 第1个child结点作为左结点 */
		pnode->right_child = huffman_child_init(pnodes[child_node--], 1);	/* 第2个child结点作为右结点 */
		pnode->left_child->parent = pnode->right_child->parent = pnode;		/* 新结点成为父结点 */
		pnode->weight = pnode->left_child->weight + pnode->right_child->weight;/* 父结点权值为2个孩子的权值之和 */
		/* 找到一个合适的插入点, 将父结点插入剩余结点组成的森林中 */
		for (i = child_node; i >= 0; i--)
		{
			/* 找到一个合适的插入点 */
			/* custom!! */
			if (pnodes[i]->weight >= pnode->weight)
				break;
		}
		/* 将新的节点插入这个位置 */
		memmove(pnodes + i + 2, pnodes + i + 1, (child_node - i) * sizeof(huffman_node_t*));
		pnodes[i + 1] = pnode;
		child_node++;
	}
	/* pnode就是根结点 */
	/* 到了这里，生成了一个按降序排列的2n - 1个结点的队列pnodes */
	huffman1_node_encode(pnode, 0, 0);

	return leaves_node;
}

int huffman_weight_compare(const void* node1, const void* node2)
{
	huffman_node_t* nodes[2] = { (huffman_node_t*)node1, (huffman_node_t*)node2 };

	/* 这里比较的前后2项顺序决定了排序是升序或降序 */
	return (int)nodes[1]->weight - (int)nodes[0]->weight;
}

int huffman_ascii_compare(const void* node1, const void* node2)
{
	huffman_node_t* nodes[2] = { (huffman_node_t*)node1, (huffman_node_t*)node2 };

	return (int)nodes[0]->ascii - (int)nodes[1]->ascii;
}

extern "C" NATIVE_API int huffman_compress(unsigned char* compr, unsigned long comprlen, unsigned char* uncompr, unsigned long uncomprlen)
{
	/* n个叶子的哈夫曼树要经过n-1次合并，产生n-1个新结点。
	 * 最终求得的哈夫曼树中共有2n-1个结点。*/
	huffman_node_t nodes[2 * 256 - 1];	/* huffman树的最大结点数(2 ^ N - 1) */
	unsigned int leaves;
	unsigned int output_bits;
	unsigned long i;
	huffman_node_t* root;
	struct bits bits;

	memset(nodes, 0, sizeof(nodes));

	/* 前256个结点(N的最大可能值)用于存放哈夫曼树的叶结点 */
	for (i = 0; i < 256; i++)
		nodes[i].ascii = (byte)i;	/* for debug: 标记该叶结点所代表的ascii值 */

	/* 计算输入的字节数据的出现频度 */
	for (i = 0; i < uncomprlen; i++)
		nodes[uncompr[i]].weight++;

	/* 按照频度（权）降序排序 */
	qsort(nodes, 256, sizeof(huffman_node_t), huffman_weight_compare);

	/* 创建huffman树 */
	leaves = huffman_tree_create(nodes);

	root = &nodes[0];
	while (root->parent)
		root = root->parent;

	bits_init(&bits, compr, comprlen);
	if (huffman_code_tree_encode(&bits, root))
		return -1;

	// sort nodes depending on ascii to can index nodes with its ascii value
	// 以便下面进行索引
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