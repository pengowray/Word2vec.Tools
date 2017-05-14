#!/usr/bin/python3

# #!/usr/bin/env python

import socket

TCP_IP = '127.0.0.1'
TCP_PORT = 5015
BUFFER_SIZE = 1024 # Normally 1024, 20 for fast response


s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((TCP_IP, TCP_PORT))
s.listen(1)

conn, addr = s.accept()
print('Connection address:', addr)

def readAll():
	data = ""
	while 1:
		try:
			part = conn.recv(BUFFER_SIZE)
			if not data: break
			print("received data:", data)
			data += part
        

			conn.send(part)  # echo
		except socket.error:
			print("Socket error occured.")
			break

	conn.close()


def linesplit(socket):
    buffer = socket.recv(4096)
    buffering = True
    while buffering:
        if "\n" in buffer:
            (line, buffer) = buffer.split("\n", 1)
            yield line + "\n"
        else:
            more = socket.recv(4096)
            if not more:
                buffering = False
            else:
                buffer += more
    if buffer:
        yield buffer		

for line in linesplit(conn):
	print("line: ", line)

print("done.")