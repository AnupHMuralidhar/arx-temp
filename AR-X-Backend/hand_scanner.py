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
    if img is None: return "NONE"

    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = hands.process(img_rgb)

    if results.multi_hand_landmarks:
        lm = results.multi_hand_landmarks[0].landmark
        
        # --- GEOMETRY CALCULATIONS ---
        
        # 1. Check if Index and Middle fingers are extended (Tips above PIP joints)
        # Note: In image coords, Y decreases as you go UP. So Tip.y < Pip.y means finger is UP.
        index_up = lm[8].y < lm[6].y
        middle_up = lm[12].y < lm[10].y
        
        # 2. Check if Ring and Pinky are curled (Tips below PIP joints)
        ring_down = lm[16].y > lm[14].y
        pinky_down = lm[20].y > lm[18].y
        
        # 3. Calculate distance for Pinch (existing logic)
        pinch_dist = math.sqrt((lm[4].x - lm[8].x)**2 + (lm[4].y - lm[8].y)**2)

        # --- GESTURE CLASSIFICATION ---

        # ✌️ PEACE SIGN -> CREATE NOTE
        if index_up and middle_up and ring_down and pinky_down:
            return "NOTE"

        # ✊ PINCH -> PAUSE
        elif pinch_dist < 0.05:
            return "PAUSE"
        
        # ✋ OPEN PALM -> PLAY (All fingers likely up)
        elif index_up and middle_up and not ring_down and not pinky_down:
            return "PLAY"

    return "NONE"