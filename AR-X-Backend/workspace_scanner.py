import base64
import cv2
import numpy as np
import time
from ultralytics import YOLO

# Load improved YOLOv8 model
model = YOLO('yolov8m.pt')  # Medium size, accurate but not too slow

# Only care about these objects
workspace_labels = {"laptop", "keyboard", "mouse", "tv", "monitor"}

# State
last_detection_time = 0
detection_interval = 1.0  # seconds
scan_completed = False  # Stop scanning once relevant object found

def decode_image(base64_data):
    try:
        jpg_original = base64.b64decode(base64_data)
        jpg_as_np = np.frombuffer(jpg_original, dtype=np.uint8)
        return cv2.imdecode(jpg_as_np, flags=1)
    except Exception as e:
        print("❌ Image decode failed:", e)
        return None

def extract_workspace_objects(results):
    found = set()
    for r in results:
        names = r.names if hasattr(r, "names") else {}
        for box in r.boxes:
            cls_id = int(box.cls.item())
            label = names.get(cls_id, f"id:{cls_id}")
            if label in workspace_labels:
                found.add(label)
    return list(found)

def detect_relevant_objects(base64_data):
    global last_detection_time, scan_completed
    now = time.time()

    if scan_completed:
        return []  # Stop scanning after first detection

    if now - last_detection_time < detection_interval:
        return []  # Throttle

    last_detection_time = now
    img = decode_image(base64_data)
    if img is None:
        return []

    try:
        results = model(img)
    except Exception as e:
        print("❌ Detection failed:", e)
        return []

    relevant = extract_workspace_objects(results)
    if relevant:
        print(f"🔍 Workspace objects detected: {', '.join(relevant)}")
        scan_completed = True  # Stop further scanning
    return relevant
