import sys, os, os.path, glob
# To ensure reproducibility, we need to follow these steps
# as explained in https://keras.io/getting-started/faq/#how-can-i-obtain-reproducible-results-using-keras-during-development
os.environ['PYTHONHASHSEED'] = '0'
keras_backend = os.environ['KERAS_BACKEND']

seed = 1343
import numpy as np
np.random.seed(seed)
import random as rn
rn.seed(seed)

if keras_backend == 'tensorflow':
    import tensorflow as tf
    session_conf = tf.ConfigProto(intra_op_parallelism_threads=1, inter_op_parallelism_threads=1)

from keras import backend as K
K.set_learning_phase(False)

if keras_backend == 'tensorflow':
    tf.set_random_seed(seed)
    sess = tf.Session(graph=tf.get_default_graph(), config=session_conf)
    K.set_session(sess)

from keras.applications import InceptionV3
from keras.applications.inception_v3 import preprocess_input
from keras.layers import Dense, GlobalAveragePooling2D
from keras.preprocessing.image import ImageDataGenerator
from keras.optimizers import SGD
from keras.models import Model

IM_WIDTH, IM_HEIGHT, IM_CHANNELS = 224, 224, 3 #fixed size for InceptionV3

def setup_base_model():
    model = InceptionV3(weights="imagenet", include_top=False, input_shape=(IM_WIDTH, IM_HEIGHT, IM_CHANNELS)) #include_top=False excludes final FC layer
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
   
def save_tf(model, folder, filename):
    """Save model in Tensorflow format    
        Args:
            model {graph_def} -- classification model
            folder {string} -- folder name
            filename {string} -- model filename
    """

    filepath = os.path.join(folder, filename)
    sess = K.get_session()
    
    outputs = ["input_1", "dense_2/Softmax"]
    constant_graph = tf.graph_util.convert_variables_to_constants(sess, sess.graph.as_graph_def(), outputs)
    tf.train.write_graph(constant_graph,folder,filename,as_text=False)
    print('saved the graph definition in tensorflow format at: ', filepath)

def train_generator(folder, batch_size=16, save_to_dir=None):
    """Build Keras training generator
    
    Args:
        folder {string} -- Training Image folder name
    
    Keyword Arguments:
        batch_size {string} -- Number of images per batch (default: {16})
        save_to_dir {string} -- Save new images in folder (default: {None})
    
    Returns:
        {DirectoryIterator} -- training generator
    """

    datagen =  ImageDataGenerator(
        preprocessing_function=preprocess_input,
        rotation_range=30,
        width_shift_range=0.2,
        height_shift_range=0.2,
        shear_range=0.2,
        zoom_range=0.2,
        horizontal_flip=True
    )

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
    """Build Keras Image Validation Generator
    
        Args:
            folder {string} -- Validation Image folder name
        
        Keyword Arguments:
            batch_size {string} -- Number of new images per batch (default: {16})
        
        Returns:
            {DirectoryIterator} -- validation generator
    """

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
    """Helper method for saving image generator artifacts (images)
    
    Args:
        data_folder {string} -- Target image folder name
        generated_folder {string} -- Target image folder name
    
    Keyword Arguments:
        batches_length {int} -- Iterations (default: {1})
    """

    generator = train_generator(data_folder, batch_size=16, save_to_dir=generated_folder)
    
    i = 0
    for batch in generator:
        i += 1
        if i > batches_length:
            break  # otherwise the generator would loop indefinitely

def save_tf(model, folder, filename):
    """Save model in Tensorflow format    
        Args:
            model {graph_def} -- classification model
            folder {string} -- folder name
            filename {string} -- model filename
    """

    filepath = os.path.join(folder, filename)
    sess = K.get_session()
    
    outputs = ["input_1", "dense_2/Softmax"]
    constant_graph = tf.graph_util.convert_variables_to_constants(sess, sess.graph.as_graph_def(), outputs)
    tf.train.write_graph(constant_graph,folder,filename,as_text=False)

    print('saved the graph definition in tensorflow format at: ', os.path.join(folder, filename))

def save_cntk(model, folder, filename):
    """Save model in CNTK format
    
        Args:
            model {model} -- classification model
            folder {string} -- folder name
            filename {string} -- model filename
    """

    import cntk as C
    model_target = os.path.join(folder, filename)
    C.combine(model.outputs).save(model_target)
    print('saved the model definition in CNTK format at: ', model_target)

def generate_labels(folder, filename):
    """Save labels file
    
    In order to build labels file, folder names inside folder are retrieved, and saved as a json array
    
    Arguments:
        folder {string} -- Images folder
        filename {string} -- Labels filename
    """
    
    #import json
    
    labels = [ item for item in os.listdir(folder) if os.path.isdir(os.path.join(folder, item)) ]
    #np.savetxt(filename, [json.dumps(labels)], fmt='%s')
    with open(filename, 'w') as fp:
        fp.writelines(map(lambda x: x+"\n", labels))


def train (train_folder, validation_folder, output_folder, batch_size=16, epochs=32):
    """Train image classification model
    
    Args:
        train_folder {string} -- images training folder
        validation_folder {string} -- images validation folder
        output_folder {string} -- model folder
        batch_size {int} -- batch size
        epochs {int} -- training iterations
    """

    nb_classes = len(glob.glob(train_folder + "/*"))
    base_model = setup_base_model()
    model      = setup_new_classifier(base_model, nb_classes)

    # transfer learning
    setup_transfer_learninig(model, base_model)

    history_tl = model.fit_generator(
        train_generator(train_folder,batch_size),
        steps_per_epoch=500//batch_size,
        epochs=epochs,
        validation_data=validation_generator(validation_folder, batch_size),
        validation_steps=100//batch_size,
        verbose=1,
        class_weight='auto')
    
    if keras_backend == 'tensorflow':
        save_tf(model, output_folder, 'model_tf.pb')
    else:
        save_cntk(model, output_folder, 'model_cntk.pb')
    
    generate_labels(train_folder, os.path.join(output_folder,'labels.txt'))
    
    return (history_tl, model)
