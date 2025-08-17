import asyncio
import websockets
from workspace_scanner import detect_relevant_objects
import json

connected_clients = set()

async def handler(websocket):
    print("🔌 Unity connected")
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
    print("🌐 WebSocket server starting at ws://0.0.0.0:8765")
    async with websockets.serve(handler, "0.0.0.0", 8765, max_size=4 * 1024 * 1024):
        await asyncio.Future()

if __name__ == "__main__":
    asyncio.run(main())
