import face_recognition
import os
import sys

if len(sys.argv) < 3:
    print("UNKNOWN")
    sys.exit()

uploaded_image_path = sys.argv[1]
known_faces_dir = sys.argv[2]

try:
    unknown_image = face_recognition.load_image_file(uploaded_image_path)
    unknown_encodings = face_recognition.face_encodings(unknown_image)
    if not unknown_encodings:
        print("UNKNOWN")
        sys.exit()
    unknown_encoding = unknown_encodings[0]
except Exception as e:
    print("UNKNOWN")
    sys.exit()

for filename in os.listdir(known_faces_dir):
    if not filename.lower().endswith((".jpg", ".jpeg", ".png")):
        continue

    try:
        known_image_path = os.path.join(known_faces_dir, filename)
        known_image = face_recognition.load_image_file(known_image_path)
        known_encodings = face_recognition.face_encodings(known_image)

        if not known_encodings:
            continue

        known_encoding = known_encodings[0]

        matches = face_recognition.compare_faces([known_encoding], unknown_encoding, tolerance=0.45)

        if matches and matches[0]:
            base_name = os.path.splitext(filename)[0]
            name = base_name.rsplit('_', 1)[0]  
            print(name)
            sys.exit()

    except Exception:
        continue

print("UNKNOWN")
