from io import BytesIO
from enum import IntEnum
from os.path import isfile
from random import randbytes
from struct import pack, unpack, calcsize
from hashlib import md5, sha1, sha256, sha512
from ctypes import Structure, BigEndianStructure, sizeof

SEEK_SET = 0
SEEK_CUR = 1
SEEK_END = 2

MD5_DIGEST_LEN = 16
SHA1_DIGEST_LEN = 20
SHA256_DIGEST_LEN = 32
SHA512_DIGEST_LEN = 64

rand_str = lambda n: randbytes(n).hex().upper()

class Endian(IntEnum):
	LITTLE = 0
	BIG = 1
	NETWORK = 2
	NATIVE = 3

class Type(IntEnum):
	BYTE = 0
	UBYTE = 1
	BYTE_ARRAY = 2
	UBYTE_ARRAY = 3
	UINT8 = 4
	UINT16 = 5
	UINT32 = 6
	UINT64 = 7
	INT8 = 8
	INT16 = 9
	INT32 = 10
	INT64 = 11
	VARINT = 12
	FLOAT32 = 13
	SINGLE = 14
	FLOAT64 = 15
	DOUBLE = 16
	STRING = 17
	CSTRING = 18
	STRUCT = 19

class ShrinkMode(IntEnum):
	START = 0
	CUR   = 1
	END   = 2

class StreamSection(object):
	offset = 0
	size = 0

	def __init__(self, offset: int, size: int) -> None:
		self.reset()
		self.offset = offset
		self.size = size

	def reset(self) -> None:
		self.offset = 0
		self.size = 0

class StreamIO(object):
	stream = None
	endian = None
	labels = {}

	# I/O functions
	read_func = None
	write_func = None

	# attributes
	can_seek = False
	can_tell = False

	def __init__(self, stream = None, endian: Endian = Endian.LITTLE):
		self.reset()
		self.set_stream(stream)
		self.set_endian(endian)
		self.set_io_funcs()

	# reset
	def reset(self) -> None:
		self.stream = None
		self.endian = None
		self.labels = {}
		self.read_func = None
		self.write_func = None
		self.can_seek = False
		self.can_tell = False

	# add with functionality
	def __enter__(self):
		return self

	def __exit__(self, exc_type, exc_val, exc_tb):
		self.close()

	# shortcuts
	def __int__(self) -> int:
		return self.tell()

	def __len__(self) -> int:
		return self.length()

	def __bytes__(self) -> bytes:
		return self.getvalue()

	def __iadd__(self, other: int) -> None:
		self.seek(self.tell() + other)

	def __isub__(self, other: int) -> None:
		self.seek(self.tell() - other)

	def __imul__(self, other: int) -> None:
		self.seek(self.tell() * other)

	def __ifloordiv__(self, other: int) -> None:
		self.seek(self.tell() // other)

	def __itruediv__(self, other: int) -> None:
		self.seek(self.tell() // other)

	def __getitem__(self, key: (int, slice)):
		if isinstance(key, slice):
			return self.read_bytes_at(key.start, key.stop - key.start)
		return self.read_byte_at(key)

	def __setitem__(self, key: (int, slice), value: (int, bytes, bytearray)) -> int:
		if isinstance(key, slice):
			return self.write_bytes_at(key.start, value)
		if isinstance(value, bytes) or isinstance(value, bytearray):
			if len(value) > 1:
				return self.write_bytes_at(key, value)
			else:
				return self.write_byte_at(key, value[0])
		else:
			return self.write_byte_at(key, value)

	# virtual file pointer
	@property
	def offset(self) -> int:
		return self.tell()

	@offset.setter
	def offset(self, value: int) -> None:
		self.seek(value)

	# utilities
	def set_stream(self, stream) -> None:
		"""
		Set stream to read/write from/to
		:param stream: The stream to interact with
		:return: None
		"""
		if stream is None:
			self.stream = BytesIO()
		elif type(stream) in [bytes, bytearray, memoryview]:
			self.stream = BytesIO(stream)
		elif type(stream) == str:
			if isfile(stream):
				self.stream = open(stream, "r+b")
			else:
				self.stream = open(stream, "wb")
		else:
			self.stream = stream
		self.can_seek = self.stream.seekable()
		self.can_tell = self.stream.seekable()

	def set_endian(self, endian: Endian) -> None:
		"""
		Set the endian you want to use for reading/writing data in the stream
		:param endian: LITTLE, BIG, NETWORK, or NATIVE
		:return: None
		"""
		endian = int(endian)
		endians = ["<", ">", "!", "@"]
		if endian in range(0, len(endians)):
			self.endian = endians[endian]

	def set_read_func(self, name: str) -> None:  #, *param_types):
		"""
		Set the function name in the stream of the read function
		:param name: The name of the read function
		:return: None
		"""
		if hasattr(self.stream, name):
			self.read_func = getattr(self.stream, name)

	def set_write_func(self, name: str) -> None:  #, *param_types):
		"""
		Set the function name in the stream of the write function
		:param name: The name of the write function
		:return: None
		"""
		if hasattr(self.stream, name):
			self.write_func = getattr(self.stream, name)

	def set_io_funcs(self, read_name: str = "read", write_name: str = "write") -> None:
		"""
		Set the read/write function names in the stream
		:param read_name: The name of the read function
		:param write_name: The name of the write function
		:return: None
		"""
		self.set_read_func(read_name)
		self.set_write_func(write_name)

	def tell(self) -> int:
		"""
		Tell the current position of the stream if supported
		:return: The position of the stream
		"""
		if self.can_tell:
			return self.stream.tell()
		raise NotImplementedError("tell isn't implemented in the specified stream!")

	def seek(self, index: int, whence: int = SEEK_SET) -> int:
		"""
		Jump to a position in the stream if supported
		:param index: The offset to jump to
		:param whence: Index is interpreted relative to the position indicated by whence (SEEK_SET, SEEK_CUR, and SEEK_END in io library)
		:return: The new absolute position
		"""
		if self.can_seek:
			return self.stream.seek(index, whence)
		raise NotImplementedError("seek isn't implemented in the specified stream!")

	def seek_start(self) -> int:
		"""
		Jump to the beginning of the stream if supported
		:return: The new absolute position
		"""
		return self.stream.seek(0)

	def seek_end(self) -> int:
		"""
		Jump to the end of the stream if supported
		:return: The new absolute position
		"""
		return self.stream.seek(0, SEEK_END)

	def length(self) -> int:
		"""
		Get the length of the stream if supported
		:return: The total length of the stream
		"""
		loc = self.tell()
		self.seek_end()
		size = self.tell()
		self.seek(loc)
		return size

	def getvalue(self) -> (bytes, bytearray):
		"""
		Get the stream's output
		:return: The stream's data as bytes or bytearray
		"""
		return self.stream.getvalue()

	def getbuffer(self) -> (bytes, bytearray):
		"""
		Get the stream's buffer
		:return: The stream's buffer as bytes or bytearray
		"""
		return self.stream.getbuffer()

	def flush(self) -> None:
		"""
		Write the data to the stream
		:return: None
		"""
		return self.stream.flush()

	def close(self) -> None:
		"""
		Close the stream
		:return: None
		"""
		self.stream.close()

	# labeling
	def get_labels(self) -> list:
		return list(self.labels.keys())

	def label_exists(self, name: str) -> bool:
		return name in self.get_labels()

	def get_label(self, name: str) -> int:
		return self.labels[name]

	def set_label(self, name: str, offset: int = None, overwrite: bool = True) -> int:
		if not overwrite and self.label_exists(name):
			name += ("_" + rand_str(4))
		if offset is not None and offset >= 0:
			loc = offset
		else:
			loc = self.tell()
		self.labels[name] = loc
		return loc

	def rename_label(self, old_name: str, new_name: str, overwrite: bool = True) -> bool:
		assert old_name != new_name, "Old and new label names shouldn't be the same"

		if self.label_exists(old_name):
			value = self.get_label(old_name)
			self.del_label(old_name)
			self.set_label(new_name, value, overwrite)
		return False

	def goto_label(self, name: str) -> int:
		return self.seek(self.labels[name])

	def del_label(self, name: str) -> int:
		return self.labels.pop(name)

	# base I/O methods
	def read(self, num: int = 0) -> (bytes, bytearray):
		if num <= 0:
			return self.read_func()
		return self.read_func(num)

	def write(self, data: (bytes, bytearray, int)) -> int:
		if type(data) == int:
			data = bytes([data])
		return self.write_func(data)

	def stream_unpack(self, fmt: str) -> (tuple, list):
		fmt = f"{self.endian}{fmt}"
		return unpack(fmt, self.read(calcsize(fmt)))

	def stream_pack(self, fmt: str, *values) -> int:
		fmt = f"{self.endian}{fmt}"
		return self.write(pack(fmt, *values))

	def stream_unpack_array(self, t: str, num: int) -> (tuple, list):
		fmt = f"{self.endian}{num}{t}"
		return unpack(fmt, self.read(calcsize(fmt)))

	def stream_pack_array(self, t: str, *values) -> int:
		fmt = f"{self.endian}{len(values)}{t}"
		return self.write(pack(fmt, *values))

	# bytes
	def read_sbyte(self) -> int:
		(val,) = self.stream_unpack("b")
		return val

	def read_sbyte_at(self, offset: int, ret: bool = True) -> int:
		loc = self.tell()
		self.seek(offset)
		output = self.read_sbyte()
		if ret:
			self.seek(loc)
		return output

	def read_sbytes(self, num: int) -> (tuple, list):
		return self.stream_unpack_array("b", num)

	def read_sbytes_at(self, offset: int, num: int, ret: bool = True) -> (tuple, list):
		loc = self.tell()
		self.seek(offset)
		output = self.read_sbytes(num)
		if ret:
			self.seek(loc)
		return output

	def write_sbyte(self, value: int) -> int:
		return self.stream_pack("b", value)

	def write_sbyte_at(self, offset: int, value: int, ret: bool = True) -> int:
		loc = self.tell()
		self.seek(offset)
		output = self.write_sbyte(value)
		if ret:
			self.seek(loc)
		return output

	def write_sbytes(self, values: (bytes, bytearray)) -> int:
		return self.stream_pack_array("b", *values)

	def write_sbytes_at(self, offset: int, values: (bytes, bytearray), ret: bool = True) -> int:
		loc = self.tell()
		self.seek(offset)
		output = self.write_sbytes(values)
		if ret:
			self.seek(loc)
		return output

	# bytes
	def read_byte(self) -> int:
		(val,) = self.stream_unpack("B")
		return val

	read_ubyte = read_byte

	def read_byte_at(self, offset: int, ret: bool = True) -> int:
		loc = self.tell()
		self.seek(offset)
		output = self.read_byte()
		if ret:
			self.seek(loc)
		return output

	read_bytes = read
	read_ubytes = read

	def read_bytes_at(self, offset: int, num: int, ret: bool = True) -> (tuple, list):
		loc = self.tell()
		self.seek(offset)
		output = self.read_bytes(num)
		if ret:
			self.seek(loc)
		return output

	read_ubytes_at = read_bytes_at

	def write_byte(self, value: int):
		return self.stream_pack("B", value)

	write_ubyte = write_byte

	def write_byte_at(self, offset: int, value: int, ret: bool = True) -> int:
		loc = self.tell()
		self.seek(offset)
		output = self.write_byte(value)
		if ret:
			self.seek(loc)
		return output

	write_bytes = write
	write_ubyte_at = write_byte_at

	def write_bytes_at(self, offset: int, values: (bytes, bytearray), ret: bool = True) -> int:
		loc = self.tell()
		self.seek(offset)
		output = self.write_bytes(values)
		if ret:
			self.seek(loc)
		return output

	write_ubytes_at = write_bytes_at

	def load_from_buffer(self, data: (bytes, bytearray)) -> int:
		return self.write_bytes(data)

	# boolean
	def read_bool(self) -> bool:
		(val,) = self.stream_unpack("?")
		return val

	def read_bool_array(self, num: int) -> tuple:
		return self.stream_unpack_array("?", num)

	def write_bool(self, value: bool) -> int:
		return self.stream_pack("?", value)

	def write_bool_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("?", *values)

	# int16/short
	def read_int16(self) -> int:
		(val,) = self.stream_unpack("h")
		return val

	read_short = read_int16

	def read_int16_array(self, num: int) -> tuple:
		return self.stream_unpack_array("h", num)

	read_short_array = read_int16_array

	def write_int16(self, value: int) -> int:
		return self.stream_pack("h", value)

	write_short = write_int16

	def write_int16_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("h", *values)

	write_short_array = write_int16_array

	# uint16/ushort
	def read_uint16(self) -> int:
		(val,) = self.stream_unpack("H")
		return val

	read_ushort = read_uint16

	def read_uint16_array(self, num: int) -> tuple:
		return self.stream_unpack_array("H", num)

	read_ushort_array = read_uint16_array

	def write_uint16(self, value: int) -> int:
		return self.stream_pack("H", value)

	write_ushort = write_uint16

	def write_uint16_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("H", *values)

	write_ushort_array = write_uint16_array

	# int32/int/long
	def read_int32(self) -> int:
		(val,) = self.stream_unpack("i")
		return val

	read_int = read_int32
	read_long = read_int32

	def read_int32_array(self, num: int) -> tuple:
		return self.stream_unpack_array("i", num)

	read_int_array = read_int32_array
	read_long_array = read_int32_array

	def write_int32(self, value: int) -> int:
		return self.stream_pack("i", value)

	write_int = write_int32
	write_long = write_int32

	def write_int32_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("i", *values)

	write_int_array = write_int32_array
	write_long_array = write_int32_array

	# uint32/uint/ulong
	def read_uint32(self) -> int:
		(val,) = self.stream_unpack("I")
		return val

	read_uint = read_uint32
	read_ulong = read_uint32

	def read_uint32_array(self, num: int) -> tuple:
		return self.stream_unpack_array("I", num)

	read_uint_array = read_uint32_array
	read_ulong_array = read_uint32_array

	def write_uint32(self, value: int) -> int:
		return self.stream_pack("I", value)

	write_uint = write_uint32
	write_ulong = write_uint32

	def write_uint32_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("I", *values)

	write_uint_array = write_uint32_array
	write_ulong_array = write_uint32_array

	# int64/longlong
	def read_int64(self) -> int:
		return self.stream_unpack("q")[0]

	read_longlong = read_int64

	def read_int64_array(self, num: int) -> tuple:
		return self.stream_unpack_array("q", num)

	read_longlong_array = read_int64_array

	def write_int64(self, value: int) -> int:
		return self.stream_pack("q", value)

	write_longlong = write_int64

	def write_int64_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("q", *values)

	write_longlong_array = write_int64_array

	# uint64/ulonglong
	def read_uint64(self) -> int:
		(val,) = self.stream_unpack("Q")
		return val

	read_ulonglong = read_uint64

	def read_uint64_array(self, num: int) -> tuple:
		return self.stream_unpack_array("Q", num)

	read_ulonglong_array = read_uint64_array

	def write_uint64(self, value: int) -> int:
		return self.stream_pack("Q", value)

	write_ulonglong = write_uint64

	def write_uint64_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("Q", *values)

	write_ulonglong_array = write_uint64_array

	# float32/single
	def read_float32(self) -> float:
		(val,) = self.stream_unpack("f")
		return val

	read_single = read_float32

	def read_float32_array(self, num: int) -> tuple:
		return self.stream_unpack_array("f", num)

	read_single_array = read_float32_array

	def write_float32(self, value: float) -> float:
		return self.stream_pack("f", value)

	write_single = write_float32

	def write_float32_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("f", *values)

	write_single_array = write_float32_array

	# float64/double
	def read_float64(self) -> float:
		(val,) = self.stream_unpack("d")
		return val

	read_double = read_float64

	def read_float64_array(self, num: int) -> tuple:
		return self.stream_unpack_array("d", num)

	read_double_array = read_float64_array

	def write_float64(self, value: float) -> float:
		return self.stream_pack("d", value)

	write_double = write_float64

	def write_float64_array(self, values: (list, tuple)) -> int:
		return self.stream_pack_array("d", *values)

	write_double_array = write_float64_array

	# varint
	def read_varint(self) -> int:
		shift = 0
		result = 0
		while True:
			i = self.read_byte()
			result |= (i & 0x7f) << shift
			shift += 7
			if not (i & 0x80):
				break
		return result

	def read_varint_array(self, num: int) -> tuple:
		return tuple([self.read_varint() for i in range(num)])

	def write_varint(self, num: int) -> int:
		buff = b""
		while True:
			towrite = num & 0x7f
			num >>= 7
			if num:
				buff += bytes([(towrite | 0x80)])
			else:
				buff += bytes([towrite])
				break
		return self.write_bytes(buff)

	def write_varint_array(self, values: (list, tuple)) -> int:
		return sum([self.write_varint(x) for x in values])

	# strings
	def read_int7(self) -> int:
		index = 0
		result = 0
		while True:
			byte_value = self.read_byte()
			result |= (byte_value & 0x7F) << (7 * index)
			if byte_value & 0x80 == 0:
				break
			index += 1
		return result

	def read_int7_array(self, num: int) -> tuple:
		return tuple([self.read_int7() for i in range(num)])

	def write_int7(self, value: int) -> int:
		data = b""
		num = value
		while num >= 0x80:
			data += bytes([((num | 0x80) & 0xFF)])
			num >>= 7
		data += bytes([num & 0xFF])
		return self.write(data)

	def write_int7_array(self, values: (list, tuple)) -> int:
		return sum([self.write_int7(x) for x in values])

	def read_string(self, encoding: str = "utf8") -> str:
		str_size = self.read_int7()
		if str_size <= 0:
			return ""
		return self.read(str_size).decode(encoding)

	def read_c_string(self, encoding: str = "utf8") -> str:
		output = b""
		while (tmp := self.read(1)) != b"\x00":
			output += tmp
		return output.rstrip(b"\x00").decode(encoding)

	read_str = read_string
	read_c_str = read_c_string

	def write_string(self, value: str, encoding: str = "utf8") -> int:
		bw = self.write_int7(len(value))
		bw += self.write(value.encode(encoding))
		return bw

	write_str = write_string

	def write_c_string(self, value: str, encoding: str = "utf8") -> int:
		return self.write(value.encode(encoding))

	write_c_str = write_c_string

	# hex
	def read_hex(self, num: int) -> (bytes, bytearray):
		return self.read(num).hex()

	def write_hex(self, value: str) -> int:
		return self.write(bytes.fromhex(value))

	# hashing
	def read_section_hash(self, offset: int, sections: (list, tuple), algo) -> bool:
		loc = self.tell()
		hasher = algo()
		self.seek(offset)
		stored = self.read(hasher.digest_size)
		for single in sections:
			assert isinstance(single, StreamSection), "Sections must be of type StreamSection"
			self.seek(single.offset)
			hasher.update(self.read(single.size))
		self.seek(loc)
		return stored == hasher.digest()

	def read_section_md5(self, offset: int, sections: (list, tuple)) -> bool:
		return self.read_section_hash(offset, sections, md5)

	def read_section_sha1(self, offset: int, sections: (list, tuple)) -> bool:
		return self.read_section_hash(offset, sections, sha1)

	def read_section_sha256(self, offset: int, sections: (list, tuple)) -> bool:
		return self.read_section_hash(offset, sections, sha256)

	def read_section_sha512(self, offset: int, sections: (list, tuple)) -> bool:
		return self.read_section_hash(offset, sections, sha512)

	def write_section_hash(self, offset: int, sections: (list, tuple), algo) -> int:
		loc = self.tell()
		hasher = algo()
		for single in sections:
			assert isinstance(single, StreamSection), "Sections must be of type StreamSection"
			self.seek(single.offset)
			hasher.update(self.read(single.size))
		self.seek(offset)
		output = self.write(hasher.digest())
		self.seek(loc)
		return output

	def write_section_md5(self, offset: int, sections: (list, tuple)) -> int:
		return self.write_section_hash(offset, sections, md5)

	def write_section_sha1(self, offset: int, sections: (list, tuple)) -> int:
		return self.write_section_hash(offset, sections, sha1)

	def write_section_sha256(self, offset: int, sections: (list, tuple)) -> int:
		return self.write_section_hash(offset, sections, sha256)

	def write_section_sha512(self, offset: int, sections: (list, tuple)) -> int:
		return self.write_section_hash(offset, sections, sha512)

	# structures/structs
	def read_struct(self, struct_type: (Structure, BigEndianStructure)) -> (Structure, BigEndianStructure):
		return struct_type.from_buffer_copy(self.read(sizeof(struct_type)))

	def read_struct_at(self, offset: int, struct_type: (Structure, BigEndianStructure), ret: bool = True) -> (Structure, BigEndianStructure):
		loc = self.tell()
		self.seek(offset)
		output = self.read_struct(struct_type)
		if ret:
			self.seek(loc)
		return output

	def write_struct(self, struct_obj: (Structure, BigEndianStructure)) -> int:
		return self.write(bytes(struct_obj))

	def write_struct_at(self, offset: int, struct_obj: (Structure, BigEndianStructure), ret: bool = True) -> int:
		loc = self.tell()
		self.seek(offset)
		output = self.write_struct(bytes(struct_obj))
		if ret:
			self.seek(loc)
		return output

	# functions
	def perform_function(self, size: int, func):
		res = func(self.read_bytes(size))
		self.write_bytes(res)
		return res

	def perform_function_at(self, offset: int, size: int, func, ret: bool = True):
		res = func(self.read_bytes_at(offset, size, ret))
		self.write_bytes_at(offset, res, ret)
		return res

	# resizing
	def expand(self, size_or_value: (int, bytes, bytearray)) -> None:
		if not self.can_seek or not self.can_tell:
			raise IOError("Stream must be seekable and tellable to expand")

		loc = self.tell()
		data = self.getvalue()
		self.stream.seek(0)
		self.stream.write(data[:loc])
		size = 0
		if type(size_or_value) == int:
			size = size_or_value
			self.stream.write((b"\x00" * size_or_value))
		elif type(size_or_value) in [bytes, bytearray]:
			size = len(size_or_value)
			self.stream.write(size_or_value)
		self.stream.write(data[loc:])
		self.stream.seek(loc)
		# update label offsets after expanding
		for label in self.get_labels():
			label_loc = self.get_label(label)
			if label_loc >= loc:
				self.set_label(label, label_loc + size)

	def shrink(self, size: int, mode: ShrinkMode = ShrinkMode.START) -> None:
		if not self.can_seek or not self.can_tell:
			raise IOError("Stream must be seekable and tellable to shrink")

		loc = self.tell()
		data = self.getvalue()
		if mode == ShrinkMode.START:
			self.set_stream(BytesIO(data[size:]))
			self.stream.seek(loc - size)

			# update label offsets after shrinking
			for label in self.get_labels():
				label_loc = self.get_label(label)
				# old offset zero doesn't exist anymore
				if label_loc in range(0, size):
					self.del_label(label)
				else:
					self.set_label(label, label_loc - size)
		elif mode == ShrinkMode.CUR:
			self.set_stream(BytesIO(data[:loc] + data[loc + size:]))
			self.stream.seek(loc - size)

			# update label offsets after shrinking
			for label in self.get_labels():
				label_loc = self.get_label(label)
				if label_loc in range(loc, len(data)):
					self.set_label(label, label_loc - size)
				elif label_loc > self.length():
					self.del_label(label)
		elif mode == ShrinkMode.END:
			self.set_stream(BytesIO(data[:len(data) - size]))
			self.stream.seek(loc)

			# update label offsets after shrinking
			for label in self.get_labels():
				label_loc = self.get_label(label)
				# remove labels in the deleted range
				if label_loc > self.length():
					self.del_label(label)