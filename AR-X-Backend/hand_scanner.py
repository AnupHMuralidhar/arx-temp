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

import math

# Helper function to calculate 2D distance
def get_dist(p1, p2):
    return math.sqrt((p1.x - p2.x)**2 + (p1.y - p2.y)**2)

def detect_gesture(base64_data):
    img = decode_image(base64_data)
    if img is None: return "NONE"

    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = hands.process(img_rgb)

    if results.multi_hand_landmarks:
        lm = results.multi_hand_landmarks[0].landmark
        
        # --- 1. ROBUST FINGER STATE DETECTION ---
        # We compare the distance of the TIP to the WRIST (0) vs the PIP (Middle Joint) to the WRIST.
        # If Tip is closer to Wrist than the middle joint is, the finger is definitely curled.
        
        # Wrist is landmark 0
        wrist = lm[0]
        
        # Check Index (Tip 8, PIP 6)
        index_curled = get_dist(lm[8], wrist) < get_dist(lm[6], wrist)
        
        # Check Middle (Tip 12, PIP 10)
        middle_curled = get_dist(lm[12], wrist) < get_dist(lm[10], wrist)
        
        # Check Ring (Tip 16, PIP 14)
        ring_curled = get_dist(lm[16], wrist) < get_dist(lm[14], wrist)
        
        # Check Pinky (Tip 20, PIP 18)
        pinky_curled = get_dist(lm[20], wrist) < get_dist(lm[18], wrist)
        
        # Fist Check (All 4 fingers curled)
        fingers_are_fist = index_curled and middle_curled and ring_curled and pinky_curled

        # Thumb State (Tip 4, IP 3, MCP 2)
        # Thumbs are tricky. We check if the tip is "far out" from the index knuckle.
        # Dist from Thumb Tip(4) to Index Knuckle(5) vs Dist from Thumb Knuckle(2) to Index Knuckle(5)
        thumb_extended = get_dist(lm[4], lm[5]) > get_dist(lm[3], lm[5])
        
        # Pinch Distance (Thumb to Index)
        pinch_dist = get_dist(lm[4], lm[8])

        # --- 2. DEBUGGING (Uncomment to see what the AI sees!) ---
        # print(f"I:{index_curled} M:{middle_curled} R:{ring_curled} P:{pinky_curled} | Fist:{fingers_are_fist}")

        # --- 3. CLASSIFY GESTURES ---

        # 👍 THUMBS UP -> NEXT
        # Logic: Fist + Thumb is extended + Thumb Tip is ABOVE Index Knuckle
        # (Note: In image Y, smaller is higher)
        if fingers_are_fist and thumb_extended and (lm[4].y < lm[5].y):
            return "NEXT"

        # 👎 THUMBS DOWN -> PREV
        # Logic: Fist + Thumb is extended + Thumb Tip is BELOW Pinky Knuckle
        elif fingers_are_fist and thumb_extended and (lm[4].y > lm[17].y):
            return "PREV"

        # ✊ PINCH / FIST -> PAUSE
        # Logic: Either a tight pinch OR a fist where the thumb isn't sticking out
        elif pinch_dist < 0.05 or (fingers_are_fist and not thumb_extended):
            return "PAUSE"

        # ✌️ PEACE SIGN -> NOTE
        # Logic: Index & Middle Extended, Ring & Pinky Curled
        elif (not index_curled) and (not middle_curled) and ring_curled and pinky_curled:
            return "NOTE"

        # ✋ OPEN PALM -> PLAY
        # Logic: All fingers extended (Not curled)
        elif not (index_curled or middle_curled or ring_curled or pinky_curled):
            return "PLAY"

    return "NONE"