from socket import socket
from socket import AF_INET
from socket import SOCK_STREAM

from struct import unpack
from struct import pack


# tcp server
class tcp_server:

	# wait for client to connect
	def connect(self, ip, port):
			print('Initializing server...')
			s = socket(AF_INET, SOCK_STREAM)
			s.bind((ip, port))
			s.listen(1)
			print('Server initialized.')
			print('Waiting for client to connect...')
			self.client, clientIp = s.accept()
			print('Client connected ', clientIp)


	# send entire packet
	def send_all(self, packet):
		size = pack('i', len(packet))
		self.client.sendall(size + packet)

	# receive entire packet
	def receive_all(self, bufferSize = 1024):
		packet = bytearray()
		packetSize = 0
		while True:
			fragment = self.client.recv(bufferSize)
			if packetSize == 0:
				packetSize = unpack('i', fragment[:4])[0]
				packet += fragment[4 : len(fragment)]
			else:
				packet += fragment
			if (len(packet) == packetSize):
				break
		return packet

	# close connection
	def close(self):
		print('Closing server...')
		self.client.close()
		print('Server closed.')