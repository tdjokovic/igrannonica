# LINEAR CONTAINS STEPS FROM CONSTRUCTING A NEURAL NETWORK
# ALL THE FUNCTIONS ARE CALLED FROM FUNCTIONS FILE

from numpy import float64
import ann.functions as fn
#import functions as fn
import pandas as pd
import matplotlib.pyplot as plt
from sklearn.preprocessing import LabelEncoder
from keras.utils import np_utils
from sklearn.preprocessing import StandardScaler

class Data():
    def __init__(self, train_file):
        self.data = train_file
        self.X_train=None
        self.X_test=None
        self.y_train=None
        self.y_test=None
        self.X_val=None
        self.y_val=None
        self.y=None
        self.X=None
        self.evaluate=None
        self.pred=None
        self.label=None


    def load_data(self, label, features):
        self.data = fn.load_data(features, label, self.data)
    

    def Misa(self, columns,enc_types,num_cat_col,type,features,label,ratio, val_test,randomize, missing_values):
        
        fn.initdict() 
        self.data = fn.load_data(features, label, self.data)
        # popunjavanje nedostajucih vrednosti
        print("DOSLI SMO DO POPUNJAVANJA NEDOSTAJUCIH")
        self.data = fn.fill_na_values(self.data, missing_values)
        X, y = fn.feature_and_label(self.data, label)
        X=fn.encode_data(X, columns,enc_types)
        X=fn.num_to_cat(X,num_cat_col)


        if (type == "regression"):
            y=pd.DataFrame(y)            

        else:
            lb=LabelEncoder()
            y=lb.fit_transform(y)
            y=np_utils.to_categorical(y)
            

        (self.X_train,self.X_val, self.X_test, self.y_train,self.y_val, self.y_test) = fn.split_data(X, y, ratio, val_test, randomize)

        self.X=X
        self.y=y

        if(type == "classification"):
            scaler=StandardScaler()
            self.X_train=scaler.fit_transform(self.X_train)
            self.X_val=scaler.fit_transform(self.X_val)
            self.X_test=scaler.fit_transform(self.X_test)
        
        else:
            self.X_train=fn.normalize(self.X_train)
            self.X_val=fn.normalize(self.X_val)
            self.X_test=fn.normalize(self.X_test)

            self.y_train=fn.normalize(self.y_train)
            self.y_val=fn.normalize(self.y_val)
            self.y_test=fn.normalize(self.y_test)
            


class Model():
    def __init__(self, data, regularization, regularization_rate):
        self.data = data
        self.model = None
        self.history = None
        self.hist = None
        self.regularization = regularization
        self.regularization_rate = regularization_rate

    def makeModel(self, type, activation_function_list, hidden_layers_n, hidden_layer_neurons_list,regularization,reg_rate,username):
        # make model
        self.model = fn.regression(self.data.X,self.data.y,type,self.data.X_train,self.data.y_train,hidden_layers_n, hidden_layer_neurons_list,activation_function_list,regularization,reg_rate,username)
              
    def compileModel(self, type,learning_rate):
        #compile the model
        fn.compile_model(self.model, type, self.data.y,learning_rate)

    def trainModel(self,type,epochs, batch_size,path):
        # train our model
        self.pred,self.label,self.evaluate,self.history = fn.train_model(self.model,type, self.data.X_train, self.data.y_train, epochs, batch_size,self.data.X_val,self.data.y_val, self.data.X_test, self.data.y_test,path)
        

    def defMetrics(self, type):
        #print("HISTORY OF TRAINING")
        #print(history)

        self.hist = dict()

        self.hist['Loss'] = self.history.history['loss']
        self.hist['valLoss'] = self.history.history['val_loss']
        self.hist['pred']=self.pred
        self.hist['label']=self.label
        self.hist['evaluate']=self.evaluate
        if (type == "regression"):
            self.hist['MAE'] = self.history.history['mae']
            self.hist['MSE'] = self.history.history['mse']
            self.hist['RMSE'] = self.history.history['root_mean_squared_error']
            self.hist['valMAE'] = self.history.history['val_mae']
            self.hist['valMSE'] = self.history.history['val_mse']
            self.hist['valRMSE'] = self.history.history['val_root_mean_squared_error']        


        if (type=="classification"):
            self.hist['Accuracy'] = self.history.history['accuracy']
            self.hist['AUC'] = self.history.history['auc']      
            self.hist['Precision'] = self.history.history['precision']
            self.hist['Recall'] = self.history.history['recall']
            self.hist['F1_score'] = self.history.history['f1_score']
            self.hist['valF1_score'] = self.history.history['val_f1_score']
            self.hist['valAccuracy'] = self.history.history['val_accuracy']  
            self.hist['valAUC'] = self.history.history['val_auc']
            self.hist['valPrecision'] = self.history.history['val_precision']
            self.hist['valRecall'] = self.history.history['val_recall']
        
