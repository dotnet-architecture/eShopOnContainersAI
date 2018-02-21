# Use the Azure Machine Learning data collector to log various metrics
from azureml.logging import get_azureml_logger
logger = get_azureml_logger()

if __name__ == '__main__':
    import sys, os

    if len(sys.argv) == 2:
        keras_backend = sys.argv[1]
    else:
        keras_backend = 'tensorflow' # valid values: cntk, tensorflow
    os.environ['KERAS_BACKEND'] = keras_backend

    from keras_training import train

    project_folder = os.path.dirname(os.path.realpath(__file__))
    print("Project folder: ", project_folder)
    output_folder = os.path.join(project_folder,'outputs')
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)
    
    data_folder = os.path.join(project_folder, 'data')
    if not os.path.exists(data_folder):
        print('Error. Path ', data_folder, ' do not exists.')

    train_folder = os.path.join(data_folder, 'train')
    if not os.path.exists(train_folder):
        print('Error. Path ', train_folder, ' do not exists.')

    validation_folder = os.path.join(data_folder, 'validation')
    if not os.path.exists(validation_folder):
        print('Error. Path ', validation_folder, ' do not exists.')

    (history_tl, model) = train(train_folder, validation_folder, output_folder, epochs=32)

    logger.log("Accuracy", history_tl.history['acc'])
    logger.log("Loss", history_tl.history['loss'])
    logger.log("Validation Accuracy", history_tl.history['val_acc'])
    logger.log("Validation Losss", history_tl.history['val_loss'])
