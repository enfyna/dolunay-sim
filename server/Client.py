import socket
import json
import cv2
import base64
import numpy as np
from time import sleep

def DrawImages(data : dict):
    print('draw start')
    im_bytes = base64.b64decode(data['cam_1'])
    im_arr = np.frombuffer(im_bytes, dtype=np.uint8)  # im_arr is one-dim Numpy array
    frame = cv2.imdecode(im_arr, flags=cv2.IMREAD_COLOR)

    im_bytes = base64.b64decode(data['cam_2'])
    im_arr = np.frombuffer(im_bytes, dtype=np.uint8)  # im_arr is one-dim Numpy array
    frame2 = cv2.imdecode(im_arr, flags=cv2.IMREAD_COLOR)

    r = TurnToRed(frame)

    cv2.imshow('frame2', frame2)
    cv2.imshow('frame', frame)

    cv2.waitKey(24)
    return r

lower_cyan = np.array([40, 50, 0], int)
upper_cyan = np.array([100, 255, 255], int)
# Renk aralıgı

def TurnToRed(cap) -> int:
	"""
	Kamerada görülen en büyük kırmızı cismi bulup
	araca ne kadar dönmesi gerektiğini söyle
	"""
	dispframe = cap
	window_x_half = cap.shape[0] / 2
	# Kameradan goruntu al
	# frame = cv2.resize(dispframe,(200,150))
	# kucult
	frame = cv2.bitwise_not(dispframe)
	# Renkleri tersine cevir
	frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
	# HSV'ye donustur
	frame = cv2.inRange(frame, lower_cyan, upper_cyan)
	# inRange ile camgobegi rengini bul
	cv2.imshow('inrange',frame)
	# frame = cv2.threshold(frame, 40, 200, cv2.THRESH_BINARY + cv2.THRESH_OTSU)[1]
	# cv2.imshow('threshold',frame)

	# cv2.imshow('cam',dispframe)
	# cv2.waitKey(24)

	contours, _ = cv2.findContours(
		frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
	# contour listesini al

	if len(contours) == 0:
		return None
		# c bulamadık o yüzden None döndür

	anlik_max_c_alani, secilen_c, scm = 0.0, None, None

	for c in contours:
		M = cv2.moments(c)
		# Moment hesabından c'nin alanını bul.

		# Anlik karedeki piksel alani en buyuk c'yi bul
		if M['m00'] > anlik_max_c_alani:
			secilen_c , scm , anlik_max_c_alani = c , M , M['m00']
			# Bulunan en buyuk c'yi anlik olarak kaydet
			# Not : scm -> secilen c momenti

	if anlik_max_c_alani < 10:
		return None

	cx = int(scm['m10'] / anlik_max_c_alani)
	# Secilen c'nin momentinden merkezini hesapla
	cv2.drawContours(dispframe,[secilen_c],-1, 0xFF0, cv2.FILLED)
	# cv2.circle(dispframe,(cx,50),1,0xFFF,3)

	cv2.imshow('cont',dispframe)
	# cisimle kameranın orta noktasındaki farkı gönder
	return int((window_x_half - cx) * 10)

class SimClient():

	server_ip = "127.0.0.1"
	server_port = 12345

	def __init__(self):
		self.client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.client_socket.connect((self.server_ip, self.server_port))
		print('Connected')

	def recv(self):
		buffer : str = ''
		while True:
			try:
				recv = self.client_socket.recv(1024)
			except:
				sleep(0.05)
				continue
			recv = recv.decode('ASCII')
			buffer += recv
			try:
				data : dict = json.loads(f"{{{buffer.split('{')[1].split('}')[0]}}}")
				buffer = ''

				if 'cam_1' in data.keys():
					im_bytes = base64.b64decode(data['cam_1'])
					im_arr = np.frombuffer(im_bytes, dtype=np.uint8)
					data['cam_1'] = cv2.imdecode(im_arr, flags=cv2.IMREAD_COLOR)
				
				if 'cam_2' in data.keys():
					im_bytes = base64.b64decode(data['cam_2'])
					im_arr = np.frombuffer(im_bytes, dtype=np.uint8)
					data['cam_2'] = cv2.imdecode(im_arr, flags=cv2.IMREAD_COLOR)

				print(f'Received data : {data.keys()}')
				return data
			except:
				print(f"Received buffer from Godot")
				continue				

	def SendData(self, data : dict):
		# data_to_send : dict = {
		# 	'is_armed' : 1,
		# 	'inputs' : [0, 0, 500, r],
		# }
		data_to_send = json.dumps(data)
		self.client_socket.send(data_to_send.encode('ASCII'))
		return

	def close(self):
		self.client_socket.close()

