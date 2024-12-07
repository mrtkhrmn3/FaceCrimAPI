import torch
from torchvision import models, transforms
from PIL import Image
import sys
import cv2
import os

# Modeli yeniden oluştur
model = models.resnet18(pretrained=False)
num_ftrs = model.fc.in_features
model.fc = torch.nn.Linear(num_ftrs, 3)  # 3 sınıf için çıktı katmanı
model_path = os.getenv("MODEL_PATH", "new_resnet18_terror_model.pth")
model.load_state_dict(torch.load("\home\site\wwwroot\predict\new_resnet18_terror_model.pth", map_location=torch.device('cpu')))
#model.load_state_dict(torch.load("C:\home\site\wwwroot\predict\new_resnet18_terror_model.pth", map_location=torch.device('cpu')))
model.eval()

# Sınıf isimleri
class_names = ['FETO', 'INNO', 'PKK']

def predict(image_path):
    # Görüntü yükle ve RGB formatına dönüştür
    image = Image.open(image_path).convert('RGB')
    
    # Siyah beyaz (grayscale) yapmak
    image = image.convert('L')  # Eğer sadece gri tonlamada tahmin yapmak istiyorsanız 'L' formatı kullanılabilir.
    
    # Yüz tespiti (isteğe bağlı)
    face_cascade = cv2.CascadeClassifier(cv2.data.haarcascades + 'haarcascade_frontalface_default.xml')
    img = cv2.imread(image_path)
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    
    faces = face_cascade.detectMultiScale(gray, scaleFactor=1.1, minNeighbors=5)
    if len(faces) > 0:
        (x, y, w, h) = faces[0]
        img_cropped = img[y:y+h, x:x+w]
        cv2.imwrite("cropped_face.jpg", img_cropped)  # Kırpılmış yüzü kaydet
        image = Image.open("cropped_face.jpg").convert('RGB')  # Yüzü tekrar RGB formatında aç

    # Görüntü dönüşümleri
    transform = transforms.Compose([
        transforms.Resize((224, 224)),
        transforms.ToTensor(),
        transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
    ])

    # Görüntüyü modele hazırla
    image = transform(image).unsqueeze(0)  # Batch boyutunu ekle (1, C, H, W)

    # Model ile tahmin yap
    outputs = model(image)
    _, preds = torch.max(outputs, 1)  # En yüksek skoru al
    return class_names[preds[0]]

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Lütfen bir görüntü dosya yolu sağlayın.")
    else:
        image_path = sys.argv[1]  # Komut satırından görüntü yolunu al
        prediction = predict(image_path)
        print(prediction)  # Tahmini standard output olarak döndür
