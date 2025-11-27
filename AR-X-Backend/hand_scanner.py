import mediapipe as mp
import cv2
import numpy as np
import base64
import math

mp_hands = mp.solutions.hands
# Use a higher confidence for gestures to avoid jitter
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=1, min_detection_confidence=0.7)

def decode_image(base64_data):
    try:
        jpg_original = base64.b64decode(base64_data)
        jpg_as_np = np.frombuffer(jpg_original, dtype=np.uint8)
        img = cv2.imdecode(jpg_as_np, flags=1)
        return img
    except Exception as e:
        return None

def detect_gesture(base64_data):
    img = decode_image(base64_data)
    if img is None:
        return "NONE"

    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = hands.process(img_rgb)

    if results.multi_hand_landmarks:
        # Get the first hand found
        hand_landmarks = results.multi_hand_landmarks[0]
        
        # --- LOGIC: CHECK FOR PINCH (PAUSE) ---
        # Landmark 4 is Thumb Tip, Landmark 8 is Index Tip
        thumb_tip = hand_landmarks.landmark[4]
        index_tip = hand_landmarks.landmark[8]

        # Calculate distance between thumb and index
        distance = math.sqrt(
            (thumb_tip.x - index_tip.x)**2 + 
            (thumb_tip.y - index_tip.y)**2 + 
            (thumb_tip.z - index_tip.z)**2
        )

        # If fingers are very close (< 0.05), it's a PINCH/FIST
        if distance < 0.05:
            return "PAUSE"
        
        # --- LOGIC: CHECK FOR OPEN PALM (PLAY) ---
        # If distance is far, we assume hand is open
        elif distance > 0.1:
            return "PLAY"

    return "NONE"