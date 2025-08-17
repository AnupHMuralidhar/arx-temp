import base64
import cv2
import numpy as np
import time
from ultralytics import YOLO

# Load improved model
model = YOLO('yolov8m.pt')  # Accurate but fast enough on PC

# Throttle detection
last_detection_time = 0
last_logged_objects = []
skip_counter = 0
detection_interval = 1.5  # seconds

# Only care about these objects
workspace_labels = {
    "laptop", "keyboard", "mouse", "tv", "monitor", "cell phone",
    "table", "chair", "couch", "bed", "desk", "refrigerator", "wall"
}

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
    global last_detection_time, last_logged_objects, skip_counter
    now = time.time()

    # Throttle detection
    if now - last_detection_time < detection_interval:
        skip_counter += 1
        if skip_counter % 10 == 0:
            print("⏳ Skipping detection...")
        return []

    skip_counter = 0  # reset
    img = decode_image(base64_data)
    if img is None:
        return []

    try:
        results = model(img)
    except Exception as e:
        print("❌ Detection failed:", e)
        return []

    last_detection_time = now
    relevant = extract_workspace_objects(results)

    if relevant != last_logged_objects:
        print(f"🔍 Workspace objects: {', '.join(relevant) if relevant else 'None'}")
        last_logged_objects = relevant
    return relevant
