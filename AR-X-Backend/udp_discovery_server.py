import socket

DISCOVERY_PORT = 37020
DISCOVERY_MESSAGE = b"ARX_DISCOVERY"
RESPONSE_MESSAGE = b"ARX_BACKEND_RESPONSE"

def run_udp_discovery_server():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind(("", DISCOVERY_PORT))
    sock.settimeout(1.0)  # 1 second timeout
    print(f"UDP discovery server listening on port {DISCOVERY_PORT}...")

    try:
        while True:
            try:
                data, addr = sock.recvfrom(1024)
                print(f"Received UDP packet from {addr}: {data}")
                if data == DISCOVERY_MESSAGE:
                    print(f"Responding to discovery from {addr}")
                    sock.sendto(RESPONSE_MESSAGE, addr)
            except socket.timeout:
                # Timeout occurred, loop again to check for KeyboardInterrupt
                continue

    except KeyboardInterrupt:
        print("\nUDP discovery server stopped by user")

    finally:
        sock.close()
        print("Socket closed")

if __name__ == "__main__":
    run_udp_discovery_server()
