
Write-Host "Initial ESHOPAI_PRODUCTSEARCHIMAGEBASED_APPROACH value: " $env:ESHOPAI_PRODUCTSEARCHIMAGEBASED_APPROACH

$env:ESHOPAI_PRODUCTSEARCHIMAGEBASED_APPROACH = "TensorFlowCustom" 

Write-Host "Final ESHOPAI_PRODUCTSEARCHIMAGEBASED_APPROACH value: " $env:ESHOPAI_PRODUCTSEARCHIMAGEBASED_APPROACH

Write-Host "Stopping and deleting the containers..."
docker-compose rm --stop --force ai.productsearchimagebased.tensorflow.api 
docker-compose rm --stop --force ai.productsearchimagebased.azurecognitiveservices.api 
docker-compose rm --stop --force webmvc 

Write-Host "Starting the containers..."
docker-compose up --no-deps -d ai.productsearchimagebased.tensorflow.api
docker-compose up --no-deps -d ai.productsearchimagebased.azurecognitiveservices.api
docker-compose up --no-deps -d webmvc



# If using containers names with 'docker' command

#docker stop ai.productsearchimagebased.tensorflow.api-container
#docker stop ai.productsearchimagebased.azurecognitiveservices.api-container
#docker stop webmvc-container

#docker rm ai.productsearchimagebased.tensorflow.api-container
#docker rm ai.productsearchimagebased.azurecognitiveservices.api-container
#docker rm webmvc-container