import sys, os, os.path, glob
import numpy as np
import cntk as C

keras_backend = 'cntk'
os.environ['KERAS_BACKEND'] = keras_backend

def save_labels():
    import os, json

    train_folder = "data/train"
    labels = [ item for item in os.listdir(train_folder) if os.path.isdir(os.path.join(train_folder, item)) ]
    numpy.savetxt("labels.json", [json.dumps(labels)], fmt='%s')

def load_cntk(model_file):
    cntk_model = C.load_model(model_file)
    return cntk_model

def rgb_norm(val):
    # this is equivalent to keras.applications.inception_v3.preprocess_input
    return 2/255*(val-255)+1

def load_image(img_file, IM_WIDTH=224, IM_HEIGHT=224):
    from keras.preprocessing import image

    # load image into numpy array, resizing if needed
    img = image.load_img(img_file, target_size=(IM_WIDTH, IM_HEIGHT))
    x = image.img_to_array(img)
    x = np.expand_dims(x, axis=0)

    x = rgb_norm(x)

    return x

def predict_image(image_filename, model, labels):
    image = load_image(image_filename)
    output = model.eval(image)
    print (output)
    print (labels[output.argmax()])

if __name__ == '__main__':
    project_folder = os.path.dirname(os.path.realpath(__file__))
    print("Project folder: ", project_folder)

    output_folder = os.path.join(project_folder,'outputs')
    data_folder = os.path.join(project_folder, 'data')
    train_folder = os.path.join(data_folder, 'train')
    validation_folder = os.path.join(data_folder, 'validation')
    nb_classes = len(glob.glob(train_folder + "/*"))

    labels = [ item for item in os.listdir(train_folder) if os.path.isdir(os.path.join(train_folder, item)) ]
    print("labels: ", labels)

    # load cntk model
    model = load_cntk(os.path.join(output_folder,'model_cntk.pb'))

    print("expected: bracelet")
    predict_image(os.path.join(validation_folder,'bracelet','3506.jpg'), model, labels)
    print("expected: parasol")
    predict_image(os.path.join(validation_folder,'parasol','114a.jpg'), model, labels)
