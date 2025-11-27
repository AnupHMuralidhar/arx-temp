import asyncio
import websockets
import socket
import json
from workspace_scanner import detect_relevant_objects
from hand_scanner import detect_gesture
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
            if data == DISCOVERY_MESSAGE:
                await loop.sock_sendto(sock, RESPONSE_MESSAGE, addr)
        except Exception as e:
            print(f"⚠️ UDP error: {e}")

# ===================== WEBSOCKET SERVER =====================
connected_clients = set()

async def ws_handler(websocket):
    print(f"🔌 Unity connected from {websocket.remote_address}")
    connected_clients.add(websocket)
    pose_sent = False  # Track if POSE has been sent

    try:
        async for message in websocket:
            if message.startswith("IMG:"):
                base64_data = message[4:]
                print(f"📩 Received camera frame (size: {len(base64_data)} bytes)")
                gesture = detect_gesture(base64_data)
                
                # 2. Send command ONLY if a valid gesture is found
                print(f"🤚 Detected Gesture: {gesture}")
                if gesture in ["PLAY", "PAUSE"]:
                    print(f"👉 Sending Gesture: {gesture}")
                    await websocket.send(f"GESTURE:{gesture}")




            if message.startswith("IMG:") and not pose_sent:
                base64_data = message[4:]
                print(f"📩 Received camera frame (size: {len(base64_data)} bytes)")
                
                relevant_objects = detect_relevant_objects(base64_data)

                if relevant_objects:
                    # Send detected labels
                    response = "SCAN:" + ",".join(relevant_objects)
                    await websocket.send(response)
                    print(f"🔍 Workspace objects detected: {', '.join(relevant_objects)}")

                    # Send a single mock position for first relevant object
                    mock_position = {"x": 0.2, "y": 0.0, "z": 1.5}
                    await websocket.send(f"POSE:{json.dumps(mock_position)}")
                    pose_sent = True  # Stop further POSE updates
            else:
                # Only print non-image messages
                if not message.startswith("IMG:"):
                    print(f"📩 Message from Unity: {message}")

    except websockets.exceptions.ConnectionClosed:
        print("❌ Unity disconnected")
    finally:
        connected_clients.remove(websocket)

async def main():
    ws_server = await websockets.serve(ws_handler, "0.0.0.0", 8765, max_size=4*1024*1024)
    udp_task = asyncio.create_task(udp_discovery_server())
    print("🌐 WebSocket server running at ws://0.0.0.0:8765")
    await asyncio.gather(ws_server.wait_closed(), udp_task)

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n🛑 Server stopped by user")
