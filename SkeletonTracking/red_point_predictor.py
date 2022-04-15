import cv2
import numpy as np
from detector import RedPointDetector

class KalmanFilter:
	kf = cv2.KalmanFilter(4,2)
	kf.measurementMatrix = np.array([[1, 0, 0, 0], [0, 1, 0, 0]], np.float32)
	kf.transitionMatrix = np.array([[1, 0, 1, 0], [0, 1, 0, 1], [0, 0, 1, 0], [0, 0, 0, 1]], np.float32)

	def predict(self, coordX, coordY):
		# Estimation
		measured = np.array([[np.float32(coordX)], [np.float32(coordY)]])
		self.kf.correct(measured)
		predicted = self.kf.predict()
		x, y = int(predicted[0]), int(predicted[1])
		return x, y


# My Laptop Webcam
frameWidth = 640
frameHeight = 480
cap = cv2.VideoCapture(0)
cap.set(3, frameWidth)
cap.set(4, frameHeight)
cap.set(10,150)

# Load detector
od = RedPointDetector()

# Load Kalman filter
kf = KalmanFilter()

while cap.isOpened():
	success, frame = cap.read()
	if success:
		redpoint_bbox = od.detect(frame)
		x, y, x2, y2 = redpoint_bbox
		cx = int((x + x2) / 2)
		cy = int((y + y2) / 2)

		predicted = kf.predict(cx, cy)
		cv2.circle(frame, (cx, cy), 10, (0, 255, 255), -1)
		cv2.circle(frame, (predicted[0], predicted[1]), 10, (255, 0, 0), 4)

		cv2.imshow("Result", frame)
		if cv2.waitKey(1) & 0xFF == ord('q'):
			break


