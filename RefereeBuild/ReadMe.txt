scripts make docker container image, for correct working you must
- Install Powershell
- Install Az module for powershell https://www.powershellgallery.com/packages/Az/10.4.1
- have docker on you pc(this scripts tested on windows 10/11)
- build Unity "Dedicated Server" with "linux" platform to "Build/server/" folder with name Venator_Quantum_Server(output executable "Venator_Quantum_Server.x86_64")
- change "isometricshooter.azurecr.io" to you docker compose registry url in login.ps1 and build.ps1
- execute "login.ps1" and authorize on opened Azure web browser page
- execute "build.ps1" and wait
- finish

folder name "server" and executable file "Venator_Quantum_Server.x86_64" have changable names, you can change this in 'build/Dockerfile' file if need