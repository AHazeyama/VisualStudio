# -*- coding: utf-8 -*-

#------------------------------------------
# name      : run
# FrameWork : Visual Studio
# function  : Python Startup
#------------------------------------------

import uvicorn

if __name__ == "__main__":
	uvicorn.run(
		"app:app",
		host="127.0.0.1",
		port=8001,				# Port:8000は禁止されている場合がある(-_-;)
		reload=True,
	)

