import sys, os, os.path, glob
import tensorflow as tf
from keras import backend as K
from keras.applications import InceptionV3
from keras.applications.inception_v3 import preprocess_input
from keras.layers import Dense, GlobalAveragePooling2D
from keras.preprocessing.image import ImageDataGenerator
from keras.optimizers import SGD
from keras.models import Model

from azureml.logging import get_azureml_logger
logger = get_azureml_logger()

def setup_dnn():
    model = InceptionV3(weights="imagenet", include_top=False) #include_top=False excludes final FC layer
    return model

def setup_transfer_learninig(model, base_model):
    """Freeze all layers and compile the model"""
    for layer in base_model.layers:
        layer.trainable = False  
    model.compile(optimizer='rmsprop', loss='categorical_crossentropy', metrics=['accuracy'])

def setup_new_classifier(base_model, nb_classes):
    """Add last layer to the convnet
        Args:
            base_model: keras model excluding top
            nb_classes: # of classes
        Returns:
            new keras model with last layer
    """
    FC_SIZE = 1024
    
    x = base_model.output
    x = GlobalAveragePooling2D()(x)
    x = Dense(FC_SIZE, activation='relu')(x) #new FC layer, random init
    predictions = Dense(nb_classes, activation='softmax')(x) #new softmax layer
    model = Model(inputs=base_model.input, outputs=predictions)
    return model

def setup_finetune(model):
    """Freeze the bottom NB_IV3_LAYERS and retrain the remaining top layers.
        note: NB_IV3_LAYERS corresponds to the top 2 inception blocks in the inceptionv3 arch
        Args:
            model: keras model
    """
    NB_IV3_LAYERS_TO_FREEZE = 172
    
    for layer in model.layers[:NB_IV3_LAYERS_TO_FREEZE]:
        layer.trainable = False
    for layer in model.layers[NB_IV3_LAYERS_TO_FREEZE:]:
        layer.trainable = True
    model.compile(optimizer=SGD(lr=0.0001, momentum=0.9), loss='categorical_crossentropy', metrics=['accuracy'])
    
def save_dnn(model, folder, filename):
    filepath = os.path.join(folder, filename)
    sess = K.get_session()
    tf.train.write_graph(sess.graph.as_graph_def(), folder, filename, as_text=False)
    print('saved the graph definition in tensorflow format at: ', filepath)

def utils_files_count(directory):
    """Get number of files by searching directory recursively"""
    if not os.path.exists(directory):
        return 0
    cnt = 0
    for r, dirs, files in os.walk(directory):
        for dr in dirs:
            cnt += len(glob.glob(os.path.join(r, dr + "/*")))
    return cnt

def train_generator(folder, batch_size=16, save_to_dir=None):
    IM_WIDTH, IM_HEIGHT = 299, 299 #fixed size for InceptionV3
    datagen =  ImageDataGenerator(
        preprocessing_function=preprocess_input,
        rotation_range=30,
        width_shift_range=0.2,
        height_shift_range=0.2,
        shear_range=0.2,
        zoom_range=0.2,
        horizontal_flip=True
    )
    #generated_folder = os.path.join(folder, '..', 'generated')
    if save_to_dir:
        if not os.path.exists(save_to_dir):
            os.makedirs(save_to_dir)
        else:
            utils_removeFilesInFolder(save_to_dir)

    generator = datagen.flow_from_directory(
        folder,
        target_size=(IM_WIDTH, IM_HEIGHT),
        batch_size=batch_size,
        save_to_dir=save_to_dir
    )
    return generator

def validation_generator(folder, batch_size=16):
    IM_WIDTH, IM_HEIGHT = 299, 299 #fixed size for InceptionV3
    datagen =  ImageDataGenerator(
        preprocessing_function=preprocess_input,
        rotation_range=45,
        width_shift_range=0,
        height_shift_range=0,
        shear_range=0,
        zoom_range=0,
        horizontal_flip=True
    )
    
    generator = datagen.flow_from_directory(
        folder,
        target_size=(IM_WIDTH, IM_HEIGHT),
        batch_size=batch_size
    )
    return generator

def utils_removeFilesInFolder(folder):
    files = glob.glob(os.path.join(folder,'*'))
    for f in files:
        os.remove(f)

def utils_generated_sample (data_folder, generated_folder, batches_length=1):
    generator = train_generator(data_folder, batch_size=16, save_to_dir=generated_folder)
    
    i = 0
    for batch in generator:
        i += 1
        if i > batches_length:
            break  # otherwise the generator would loop indefinitely

if __name__ == '__main__':
    project_folder = os.path.dirname(os.path.realpath(__file__))
    print("Project folder: ", project_folder)
    data_folder = os.path.join(project_folder, 'data')
    train_folder = os.path.join(data_folder, 'train')
    validation_folder = os.path.join(data_folder, 'validation')
    output_folder = os.path.join(project_folder,'output')
    model_filename = 'model.pb'

    nb_classes = len(glob.glob(train_folder + "/*"))
    batch_size=16

    # setup model
    base_model = setup_dnn()
    model      = setup_new_classifier(base_model, nb_classes)

    # transfer learning
    setup_transfer_learninig(model, base_model)

    history_tl = model.fit_generator(
        train_generator(train_folder,batch_size),
        steps_per_epoch=500//batch_size, #utils_files_count(train_folder)//batch_size,
        epochs=32,
        validation_data=validation_generator(validation_folder, batch_size),
        validation_steps=100//batch_size, #utils_files_count(validation_folder)//batch_size,
        verbose=1,
        class_weight='auto')
    
    save_dnn(model, output_folder, model_filename)

    logger.log("Accuracy", history_tl.history['acc'])
    logger.log("Loss", history_tl.history['loss'])
    logger.log("Validation Accuracy", history_tl.history['val_acc'])
    logger.log("Validation Losss", history_tl.history['val_loss'])


