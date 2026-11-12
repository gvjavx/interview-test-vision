#!/usr/bin/env python3
import base64
import os
import socket
import sys
import urllib.error
import urllib.request

HOST = "localhost"
PORT = 5001
if len(sys.argv) > 1:
    HOST, port_text = sys.argv[1].split(":")
    PORT = int(port_text)

failures = 0


def check(name, passed):
    global failures
    if passed:
        print("ok   " + name)
    else:
        print("FAIL " + name)
        failures = failures + 1


def get_status(path):
    try:
        response = urllib.request.urlopen("http://" + HOST + ":" + str(PORT) + path)
        return response.status
    except urllib.error.HTTPError as error:
        return error.code


def read_bytes(sock, count):
    data = b""
    while len(data) < count:
        chunk = sock.recv(count - len(data))
        if chunk == b"":
            raise Exception("connection closed")
        data = data + chunk
    return data


def open_websocket():
    sock = socket.create_connection((HOST, PORT), timeout=5)
    key = base64.b64encode(os.urandom(16)).decode()

    request = "GET /ws HTTP/1.1\r\n"
    request += "Host: " + HOST + "\r\n"
    request += "Upgrade: websocket\r\n"
    request += "Connection: Upgrade\r\n"
    request += "Sec-WebSocket-Key: " + key + "\r\n"
    request += "Sec-WebSocket-Version: 13\r\n"
    request += "\r\n"
    sock.sendall(request.encode())

    response = b""
    while not response.endswith(b"\r\n\r\n"):
        response = response + read_bytes(sock, 1)
    if not response.startswith(b"HTTP/1.1 101"):
        raise Exception("websocket handshake failed")
    return sock


def send_message(sock, text):
    data = text.encode()
    if len(data) >= 126:
        raise Exception("message too long for this test")

    mask = os.urandom(4)
    masked_data = bytearray()
    for i in range(len(data)):
        masked_data.append(data[i] ^ mask[i % 4])

    frame = bytes([0x81, 0x80 + len(data)]) + mask + bytes(masked_data)
    sock.sendall(frame)


def receive_message(sock):
    header = read_bytes(sock, 2)
    length = header[1] & 0x7F
    return read_bytes(sock, length).decode()


check("GET /health is 200", get_status("/health") == 200)
check("GET /ws without upgrade is 400", get_status("/ws") == 400)

alice = open_websocket()
send_message(alice, "alice joined")
check("sender gets its own message", receive_message(alice) == "alice joined")

bob = open_websocket()
send_message(bob, "hello from bob")
check("second client gets its own message", receive_message(bob) == "hello from bob")
check("first client gets the other client's message", receive_message(alice) == "hello from bob")

alice.close()
bob.close()

print(str(failures) + " failure(s)")
if failures > 0:
    sys.exit(1)
