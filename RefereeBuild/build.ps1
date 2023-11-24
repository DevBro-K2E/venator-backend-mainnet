$tag = 'is-server'
$repoRegName = 'server'
$registry = 'isometricshooter.azurecr.io'


Set-Location 'Build'

docker build -t $tag .
docker tag $tag $registry/$repoRegName
docker push $registry/$repoRegName

Set-Location '..'