import base64

import cv2
import numpy as np

import test_mission

def main(data : dict) -> list:	
	im_bytes = base64.b64decode(data['cam_1'])
	im_arr = np.frombuffer(im_bytes, dtype=np.uint8)  # im_arr is one-dim Numpy array
	frame = cv2.imdecode(im_arr, flags=cv2.IMREAD_COLOR)

	im_bytes = base64.b64decode(data['cam_2'])
	im_arr = np.frombuffer(im_bytes, dtype=np.uint8)  # im_arr is one-dim Numpy array
	frame2 = cv2.imdecode(im_arr, flags=cv2.IMREAD_COLOR)

	cv2.imshow('frame2', frame2)
	cv2.imshow('frame', frame)

	r = test_mission.TurnToRed(frame)
	if r == None:
		r = 0
	
	cv2.waitKey(24)

	return [0, 0, 500, -r]