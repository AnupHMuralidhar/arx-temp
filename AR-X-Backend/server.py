import asyncio
import websockets

async def handler(websocket):
    # This line runs when a client connects.
    print(f"✅ Client connected from {websocket.remote_address}")
    try:
        # This loop now waits for messages and prints them to the terminal.
        async for message in websocket:
            print(f"Received message: {message}")
    except websockets.ConnectionClosed:
        print(f"Client disconnected.")
    finally:
        # This runs when the connection is closed for any reason.
        print(f"Connection closed for {websocket.remote_address}")

async def main():
    # Start the server on all addresses (0.0.0.0) on port 5001.
    async with websockets.serve(handler, "0.0.0.0", 5001):
        print("🚀 Server started on ws://0.0.0.0:5001")
        await asyncio.Future()  # run forever

if __name__ == "__main__":
    asyncio.run(main())