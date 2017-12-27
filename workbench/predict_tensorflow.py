import sys, os, os.path, glob
import tensorflow as tf
import numpy

keras_backend = 'tensorflow'
os.environ['KERAS_BACKEND'] = keras_backend

def load_graph(frozen_graph_filename):
    # We load the protobuf file from the disk and parse it to retrieve the 
    # unserialized graph_def
    with tf.gfile.GFile(frozen_graph_filename, "rb") as f:
        graph_def = tf.GraphDef()
        graph_def.ParseFromString(f.read())

    # Then, we import the graph_def into a new Graph and returns it 
    with tf.Graph().as_default() as graph:
        tf.import_graph_def(graph_def, name="")
    return graph

def rgb_norm(val):
    # this is equivalent to keras.applications.inception_v3.preprocess_input
    return 2/255*(val-255)+1

def load_image(img_path, IM_WIDTH=224, IM_HEIGHT=224):
    from keras.preprocessing import image

    # load image into numpy array, resizing if needed
    img = image.load_img(img_path, target_size=(IM_WIDTH, IM_HEIGHT))
    x = image.img_to_array(img)
    x = numpy.expand_dims(x, axis=0)

    x = rgb_norm(x)

    return x

# custom inception
def predict(graph, image):
    with tf.Session(graph=graph) as sess:
        input_tensor = sess.graph.get_tensor_by_name('input_1:0')
        output = sess.run('dense_2/Softmax:0',feed_dict ={input_tensor:image})
        return output

def predict_image(image_filename, model, labels):
    image = load_image(image_filename)
    preds = predict(model, image)
    print (preds)
    print (labels[preds.argmax()])


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

    # load graph (model)
    graph = load_graph(os.path.join(output_folder,'model.pb'))

    print("expected: bracelet")
    predict_image(os.path.join(validation_folder,'bracelet','3506.jpg'), graph, labels)
    print("expected: parasol")
    predict_image(os.path.join(validation_folder,'parasol','114a.jpg'), graph, labels)

