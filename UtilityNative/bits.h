#pragma once

#include "def.h"
#include <string.h>

struct bits
{
	ulong curbits;
	ulong curbyte;
	byte cache;
	byte* stream;
	ulong stream_length;
};

void bits_init(struct bits* bits, byte* stream, ulong stream_length)
{
	memset(bits, 0, sizeof(*bits));
	bits->stream = stream;
	bits->stream_length = stream_length;
}

int bits_get_high(struct bits* bits, uint req_bits, uint* retval)
{
	uint bits_value = 0;

	while (req_bits > 0)
	{
		uint req;

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
	return bits_get_high(bits, 1, (uint*)retval);
}

int bit_put_high(struct bits* bits, byte setval)
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

int bits_put_high(struct bits* bits, uint req_bits, void* setval)
{
	uint this_bits;
	uint this_byte;
	uint i;

	this_byte = req_bits / 8;
	this_bits = req_bits & 7;
	for (int k = (int)this_bits - 1; k >= 0; k--)
	{
		byte bitval;

		bitval = !!(((byte*)setval)[this_byte] & (1 << k));
		if (bit_put_high(bits, bitval))
			return -1;
	}
	this_bits = req_bits & ~7;
	this_byte--;
	for (i = 0; i < this_bits; i++)
	{
		byte bitval;

		bitval = !!(((byte*)setval)[this_byte - i / 8] & (1 << (7 - (i & 7))));
		if (bit_put_high(bits, bitval))
			return -1;
	}

	return 0;
}

void bits_flush(struct bits* bits)
{
	bits->stream[bits->curbyte] = bits->cache;
}
