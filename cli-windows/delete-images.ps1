$imagesToDelete = docker images --filter=reference="eshopai/*" -q

If (-Not $imagesToDelete) {Write-Host "Not deleting eShopAI images as there are no eShopAI images in the current local Docker repo."} 
Else 
{
    # Delete all containers
    Write-Host "Deleting all containers in local Docker Host"
    docker rm $(docker ps -a -q) -f
    
    # Delete all eshop images
    Write-Host "Deleting eShopAI images in local Docker repo"
    Write-Host $imagesToDelete
    docker rmi $(docker images --filter=reference="eshopai/*" -q) -f
}


# DELETE ALL IMAGES AND CONTAINERS

# Delete all containers
# docker rm $(docker ps -a -q) -f

# Delete all images
# docker rmi $(docker images -q)

#Filter by image name (Has to be complete, cannot be a wildcard)
#docker ps -q  --filter=ancestor=eshop/identity.api:dev

