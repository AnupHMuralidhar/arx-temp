import asyncio
import websockets
import socket
import json
from workspace_scanner import detect_relevant_objects

# ===================== UDP DISCOVERY =====================
DISCOVERY_PORT = 37020
DISCOVERY_MESSAGE = b"ARX_DISCOVERY"
RESPONSE_MESSAGE = b"ARX_BACKEND_RESPONSE"

async def udp_discovery_server():
    loop = asyncio.get_running_loop()
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind(("", DISCOVERY_PORT))
    sock.setblocking(False)

    print(f"📡 UDP discovery server listening on port {DISCOVERY_PORT}...")

    while True:
        try:
            data, addr = await loop.sock_recvfrom(sock, 1024)
            print(f"📩 UDP packet from {addr}: {data}")
            if data == DISCOVERY_MESSAGE:
                print(f"📡 Responding to discovery from {addr}")
                await loop.sock_sendto(sock, RESPONSE_MESSAGE, addr)
        except Exception as e:
            print(f"⚠️ UDP error: {e}")


# ===================== WEBSOCKET SERVER =====================
connected_clients = set()

async def ws_handler(websocket):
    print(f"🔌 Unity connected from {websocket.remote_address}")
    connected_clients.add(websocket)
    try:
        async for message in websocket:
            if message.startswith("IMG:"):
                base64_data = message[4:]
                relevant_objects = detect_relevant_objects(base64_data)

                if relevant_objects is not None:
                    # Send detected labels back
                    response = "SCAN:" + ",".join(relevant_objects)
                    await websocket.send(response)

                    if any(obj in relevant_objects for obj in ["laptop", "tv", "monitor", "keyboard", "mouse"]):
                        # ✅ Correct message format expected by Unity
                        mock_position = {
                            "x": 0.2,
                            "y": 0.0,
                            "z": 1.5
                        }
                        json_payload = json.dumps(mock_position)
                        await websocket.send(f"POSE:{json_payload}")
            else:
                print(f"📩 Message from Unity: {message}")
    except websockets.exceptions.ConnectionClosed:
        print("❌ Unity disconnected")
    finally:
        connected_clients.remove(websocket)


async def main():
    # Start both UDP + WebSocket together
    ws_server = await websockets.serve(ws_handler, "0.0.0.0", 8765, max_size=4 * 1024 * 1024)
    print("🌐 WebSocket server running at ws://0.0.0.0:8765")

    udp_task = asyncio.create_task(udp_discovery_server())

    await asyncio.gather(ws_server.wait_closed(), udp_task)


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n🛑 Server stopped by user")
