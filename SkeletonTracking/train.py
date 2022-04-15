import cv2
import json
import lmdb
import numpy as np
import os.path
import scipy.io as sio
import struct
import sys
import caffe


def generateLmdbFile(lmdb_path, img_folder, json_file, caffee_path, mask_folder = None):
    print('Creating ' + lmdb_path + ' from ' + json_file)
    sys.path.insert(0, caffee_path)

    env = lmdb.open(lmdb_path, map_size=int(1e12))
    tx_node = env.begin(write=True)

    data = json.load(open(json_file))['root']

    count_total_write = len(data)
    print('Number training images: %d' % count_total_write)
    writeCount = 0
    rdn = np.random.permutation(count_total_write).tolist()
    if "face70_mask_out" in data[0]['dataset']:
        min_width = 300
    else:
        min_width = 128
    print_iterations = max(1, round(count_total_write / 100))

    for sample in range(count_total_write):
        if sample % print_iterations == 0:
            print('Sample %d of %d' % (sample+1, count_total_write))
        index = rdn[sample]
        is_body_mpii = ("MPII" in data[index]['dataset'] and len(data[index]['dataset']) == 4)
        maskMiss = None
        # Read image and maskMiss (if COCO)
        if "COCO" in data[index]['dataset'] \
            or "MPII_hand" in data[index]['dataset'] \
            or "mpii-hand" in data[index]['dataset'] \
            or is_body_mpii \
            or "panoptics" in data[index]['dataset'] \
            or "car14" in data[index]['dataset'] \
            or "car22" in data[index]['dataset']:
            if "COCO" in data[index]['dataset'] or is_body_mpii or "car22" in data[index]['dataset']:
                if not mask_folder:
                    mask_folder = img_folder
                # Car22
                if is_body_mpii or "car22" in data[index]['dataset']:
                    if is_body_mpii:
                        imageFullPath = os.path.join(img_folder, data[index]['img_paths']);
                    else:
                        imageFullPath = os.path.join(img_folder, data[index]['img_paths'][1:])
                    maskFileName = os.path.splitext(os.path.split(data[index]['img_paths'])[1])[0];
                    maskMissFullPath = mask_folder + maskFileName + '.png'
                else:
                    imageIndex = data[index]['img_paths'][-16:-4];
                    # COCO 2014 (e.g. foot)
                    if "2014/COCO_" in data[index]['img_paths']:
                        if "train2014" in data[index]['img_paths']:
                            kindOfData = 'train2014';
                        else:
                            kindOfData = 'val2014';
                        imageFullPath = os.path.join(img_folder, 'train2017', imageIndex + '.jpg');
                        kindOfMask = 'mask2014'
                        maskMissFullPath = mask_folder + 'mask2014/' + kindOfData + '_mask_miss_' + imageIndex + '.png'
                    # COCO 2017
                    else:
                        kindOfData = 'train2017';
                        imageFullPath = os.path.join(img_folder, kindOfData + '/' + data[index]['img_paths']);
                        kindOfMask = 'mask2017'
                        maskMissFullPath = mask_folder + kindOfMask + '/' + kindOfData + '/' + imageIndex + '.png'
                # Read image and maskMiss
                if not os.path.exists(imageFullPath):
                    raise Exception('Not found image: ' + imageFullPath)
                image = cv2.imread(imageFullPath)
                if not os.path.exists(maskMissFullPath):
                    raise Exception('Not found image: ' + maskMissFullPath)
                maskMiss = cv2.imread(maskMissFullPath, 0) # 0 = Load grayscale image
            # MPII or car14
            else:
                imageFullPath = os.path.join(img_folder, data[index]['img_paths']);
                image = cv2.imread(imageFullPath)
                # # Debug - Display image
                # print(imageFullPath)
                # cv2.imshow("image", image)
                # cv2.waitKey(0)
        elif "face70" in data[index]['dataset'] \
            or "hand21" in data[index]['dataset'] \
            or "hand42" in data[index]['dataset']:
            imageFullPath = os.path.join(img_folder, data[index]['image_path'])
            image = cv2.imread(imageFullPath)
            if "face70_mask_out" in data[0]['dataset']:
                kindOfMask = 'mask2017'
                maskMissFullPath = mask_folder + data[index]['image_path'][:-4] + '.png'
                if not os.path.exists(maskMissFullPath):
                    raise Exception('Not found image: ' + maskMissFullPath)
                maskMiss = cv2.imread(maskMissFullPath, 0) # 0 = Load grayscale image
            elif "face70" not in data[index]['dataset']:
                kindOfMask = 'mask2017'
                maskMissFullPath = mask_folder + kindOfMask + '/' + data[index]['dataset'][:6] + '/' + data[index]['image_path'][:-4] + '.png'
                if not os.path.exists(maskMissFullPath):
                    raise Exception('Not found image: ' + maskMissFullPath)
                maskMiss = cv2.imread(maskMissFullPath, 0) # 0 = Load grayscale image
        elif "dome" in data[index]['dataset']:
            # No maskMiss for "dome" dataset
            pass
        else:
            raise Exception('Unknown dataset called ' + data[index]['dataset'] + '.')

        # COCO / MPII
        if "COCO" in data[index]['dataset'] \
            or is_body_mpii \
            or "face70" in data[index]['dataset'] \
            or "hand21" in data[index]['dataset'] \
            or "hand42" in data[index]['dataset'] \
            or "MPII_hand" in data[index]['dataset'] \
            or "mpii-hand" in data[index]['dataset'] \
            or "panoptics" in data[index]['dataset'] \
            or "car14" in data[index]['dataset'] \
            or "car22" in data[index]['dataset']:
            try:
                height = image.shape[0]
                width = image.shape[1]
                # print("Image size: "+ str(width) + "x" + str(height))
            except:
                print('Image not found at ' + imageFullPath)
                height = image.shape[0]
            if width < min_width:
                image = cv2.copyMakeBorder(image,0,0,0,min_width-width,cv2.BORDER_CONSTANT,value=(128,128,128))
                if maskMiss is not None:
                    maskMiss = cv2.copyMakeBorder(maskMiss,0,0,0,min_width-width,cv2.BORDER_CONSTANT,value=(0,0,0))
                width = min_width
                # Note: width parameter not modified, we want to keep information
            metaData = np.zeros(shape=(height,width,1), dtype=np.uint8)
        # Dome
        elif "dome" in data[index]['dataset']:
            # metaData = np.zeros(shape=(100,200), dtype=np.uint8) # < 50 keypoints
            # metaData = np.zeros(shape=(100,59*4), dtype=np.uint8) # 59 keypoints (body + hand)
            metaData = np.zeros(shape=(100,135*4), dtype=np.uint8) # 135 keypoints
        else:
            raise Exception('Unknown dataset!')
        # dataset name (string)
        currentLineIndex = 0
        for i in range(len(data[index]['dataset'])):
            metaData[currentLineIndex][i] = ord(data[index]['dataset'][i])
        currentLineIndex = currentLineIndex + 1
        # image height, image width
        heightBinary = float2bytes(float(data[index]['img_height']))
        for i in range(len(heightBinary)):
            metaData[currentLineIndex][i] = ord(heightBinary[i])
        widthBinary = float2bytes(float(data[index]['img_width']))
        for i in range(len(widthBinary)):
            metaData[currentLineIndex][4 + i] = ord(widthBinary[i])
        currentLineIndex = currentLineIndex + 1
        # (a) numOtherPeople (uint8), people_index (uint8), annolist_index (float), writeCount(float), count_total_write(float)
        metaData[currentLineIndex][0] = data[index]['numOtherPeople']
        metaData[currentLineIndex][1] = data[index]['people_index']
        annolistIndexBinary = float2bytes(float(data[index]['annolist_index']))
        for i in range(len(annolistIndexBinary)): # 2,3,4,5
            metaData[currentLineIndex][2 + i] = ord(annolistIndexBinary[i])
        countBinary = float2bytes(float(writeCount)) # note it's writecount instead of sample!
        for i in range(len(countBinary)):
            metaData[currentLineIndex][6 + i] = ord(countBinary[i])
        count_total_writeBinary = float2bytes(float(count_total_write))
        for i in range(len(count_total_writeBinary)):
            metaData[currentLineIndex][10 + i] = ord(count_total_writeBinary[i])
        numberOtherPeople = int(data[index]['numOtherPeople'])
        currentLineIndex = currentLineIndex + 1
        # (b) objpos_x (float), objpos_y (float)
        objposBinary = float2bytes(data[index]['objpos'])
        for i in range(len(objposBinary)):
            metaData[currentLineIndex][i] = ord(objposBinary[i])
        currentLineIndex = currentLineIndex + 1
        # try:
        # (c) scale_provided (float)
        scaleProvidedBinary = float2bytes(float(data[index]['scale_provided']))
        for i in range(len(scaleProvidedBinary)):
            metaData[currentLineIndex][i] = ord(scaleProvidedBinary[i])
        currentLineIndex = currentLineIndex + 1
        # (d) joint_self (3*#keypoints) (float) (3 line)
        joints = np.asarray(data[index]['joint_self']).T.tolist() # transpose to 3*#keypoints
        for i in range(len(joints)):
            rowBinary = float2bytes(joints[i])
            for j in range(len(rowBinary)):
                metaData[currentLineIndex][j] = ord(rowBinary[j])
            currentLineIndex = currentLineIndex + 1
        # (e) check numberOtherPeople, prepare arrays
        if numberOtherPeople!=0:
            # If generated with Matlab JSON format
            if "COCO" in data[index]['dataset'] \
                or "car22" in data[index]['dataset']:
                if numberOtherPeople==1:
                    jointOthers = [data[index]['joint_others']]
                    objposOther = [data[index]['objpos_other']]
                    scaleProvidedOther = [data[index]['scale_provided_other']]
                else:
                    jointOthers = data[index]['joint_others']
                    objposOther = data[index]['objpos_other']
                    scaleProvidedOther = data[index]['scale_provided_other']
            elif "dome" in data[index]['dataset'] \
                or is_body_mpii \
                or "face70" in data[index]['dataset'] \
                or "hand21" in data[index]['dataset'] \
                or "hand42" in data[index]['dataset'] \
                or "MPII_hand" in data[index]['dataset'] \
                or "car14" in data[index]['dataset']:
                jointOthers = data[index]['joint_others']
                objposOther = data[index]['objpos_other']
                scaleProvidedOther = data[index]['scale_provided_other']
            else:
                raise Exception('Unknown dataset!')
            # (f) objpos_other_x (float), objpos_other_y (float) (numberOtherPeople lines)
            for i in range(numberOtherPeople):
                objposBinary = float2bytes(objposOther[i])
                for j in range(len(objposBinary)):
                    metaData[currentLineIndex][j] = ord(objposBinary[j])
                currentLineIndex = currentLineIndex + 1
            # (g) scaleProvidedOther (numberOtherPeople floats in 1 line)
            scaleProvidedOtherBinary = float2bytes(scaleProvidedOther)
            for j in range(len(scaleProvidedOtherBinary)):
                metaData[currentLineIndex][j] = ord(scaleProvidedOtherBinary[j])
            currentLineIndex = currentLineIndex + 1
            # (h) joint_others (3*#keypoints) (float) (numberOtherPeople*3 lines)
            for n in range(numberOtherPeople):
                joints = np.asarray(jointOthers[n]).T.tolist() # transpose to 3*#keypoints
                for i in range(len(joints)):
                    rowBinary = float2bytes(joints[i])
                    for j in range(len(rowBinary)):
                        metaData[currentLineIndex][j] = ord(rowBinary[j])
                    currentLineIndex = currentLineIndex + 1
        # (i) img_paths
        if "dome" in data[index]['dataset'] and "hand21" not in data[index]['dataset'] \
            and "hand42" not in data[index]['dataset']:
            # for i in range(len(data[index]['img_paths'])):
            #     metaData[currentLineIndex][i] = ord(data[index]['img_paths'][i])
            for i in range(len(data[index]['image_path'])):
                metaData[currentLineIndex][i] = ord(data[index]['image_path'][i])
            currentLineIndex = currentLineIndex + 1

        # # (j) depth enabled(uint8)
        # if "dome" in data[index]['dataset'] and "hand21" not in data[index]['dataset'] \
        #     and "hand42" not in data[index]['dataset']:
        #     metaData[currentLineIndex][0] = data[index]['depth_enabled']
        #     currentLineIndex = currentLineIndex + 1

        # # (k) depth_path
        # if "dome" in data[index]['dataset'] and "hand21" not in data[index]['dataset'] \
        #     and "hand42" not in data[index]['dataset']:
        #     if data[index]['depth_enabled']>0:
        #         for i in range(len(data[index]['depth_path'])):
        #             metaData[currentLineIndex][i] = ord(data[index]['depth_path'][i])
        #         currentLineIndex = currentLineIndex + 1

        # COCO: total 7 + 4*numberOtherPeople lines
        # DomeDB: X lines
        # If generated with Matlab JSON format
        if "COCO" in data[index]['dataset'] \
            or "hand21" in data[index]['dataset'] \
            or "hand42" in data[index]['dataset'] \
            or is_body_mpii \
            or "car22" in data[index]['dataset'] \
            or "face70_mask_out" in data[index]['dataset']:
            dataToSave = np.concatenate((image, metaData, maskMiss[...,None]), axis=2)
            dataToSave = np.transpose(dataToSave, (2, 0, 1))
        elif "face70" in data[index]['dataset'] \
            or "MPII_hand" in data[index]['dataset'] \
            or "mpii-hand" in data[index]['dataset'] \
            or "panoptics" in data[index]['dataset'] \
            or "car14" in data[index]['dataset']:
            dataToSave = np.concatenate((image, metaData), axis=2)
            dataToSave = np.transpose(dataToSave, (2, 0, 1))
        elif "dome" in data[index]['dataset']:
            dataToSave = np.transpose(metaData[:,:,None], (2, 0, 1))
        else:
            raise Exception('Unknown dataset!')

        datum = caffe.io.array_to_datum(dataToSave, label=0)
        key = '%07d' % writeCount
        tx_node.put(key, datum.SerializeToString())

        if writeCount % 500 == 0:
            tx_node.commit()
            tx_node = env.begin(write=True)
        print('%d/%d/%d/%d' % (sample, writeCount, index, count_total_write))
        writeCount = writeCount + 1

    tx_node.commit()
    env.close()

def generateNegativesLmdbFile(lmdb_path, img_folder, json_file, caffee_path):
    sys.path.insert(0, caffee_path)
    import caffe

    env = lmdb.open(lmdb_path, map_size=int(1e12))
    tx_node = env.begin(write=True)

    data = json.load(open(json_file))
    count_total_write = len(data)
    print('%d samples' % (count_total_write))
    writeCount = 0
    rdn = np.random.permutation(count_total_write).tolist()
    print_iterations = max(1, round(count_total_write / 100))

    for sample in range(count_total_write):
        index = rdn[sample]
        if sample % print_iterations == 0:
            print('Sample %d of %d' % (sample+1, count_total_write))
        # Read image
        imageFilePath = os.path.join(img_folder, data[rdn[sample]])
        image = cv2.imread(imageFilePath)
        if image.shape[0] + image.shape[1] < 1:
            errorMessage = 'Image not found! ' + imageFilePath
            raise Exception(errorMessage)
        # Save image
        dataToSave = np.transpose(image, (2, 0, 1))
        datum = caffe.io.array_to_datum(dataToSave, label=0)
        key = '%07d' % writeCount
        tx_node.put(key, datum.SerializeToString())
        if writeCount % 2500 == 0:
            tx_node.commit()
            tx_node = env.begin(write=True)
        writeCount = writeCount + 1
    tx_node.commit()
    env.close()

def float2bytes(floats):
    if type(floats) is float:
        floats = [floats]
    else:
        for i in range(len(floats)):
            floats[i] = float(floats[i])
    # Make sure they are all floats
    return struct.pack('%sf' % len(floats), *floats)


def bytes2float(bytes):
    return struct.unpack('%sf' % len(bytes), bytes)[0]

def train(batchSizes, layerName, numberOutputChannels,
                    trainedModelsFolder,  lrMultDistro, caffeFolder, pretrainedModelPath,
                     numberIterations):
    # start training
    caffe.set_mode_gpu()
    caffe.set_device(0)
    
    # Create the network
    net = caffe.Net(os.path.join(caffeFolder, 'train_val_lmdb_' + layerName + '.prototxt'),
                    pretrainedModelPath,
                    caffe.TRAIN)
    net.copy_from(os.path.join(caffeFolder, 'train_val_lmdb_' + layerName + '.caffemodel'))

    # Set the learning rate
    for param_name in net.params.keys():
        param = net.params[param_name]
        for i in range(len(param)):
            if param_name == 'conv1_1_1x1_bn':
                param[i].lr_mult = lrMultDistro[0]
            elif param_name == 'conv1_1_1x1_bn_scale':
                param[i].lr_mult = lrMultDistro[1]
            elif param_name == 'conv1_1_1x1_bn_bias':
                param[i].lr_mult = lrMultDistro[2]
            elif param_name == 'conv1_1_1x1_bn_running_mean':
                param[i].lr_mult = lrMultDistro[3]
            elif param_name == 'conv1_1_1x1_bn_running_var':
                param[i].lr_mult = lrMultDistro[4]
            elif param_name == 'conv1_1_1x1_bn_sparse':
                param[i].lr_mult = lrMultDistro[5]
            elif param_name == 'conv1_1_1x1_bn_sparse_scale':
                param[i].lr_mult = lrMultDistro[6]
            elif param_name == 'conv1_1_1x1_bn_sparse_bias':
                param[i].lr_mult = lrMultDistro[7]
            elif param_name == 'conv1_1_1x1_bn_sparse_running_mean':
                param[i].lr_mult = lrMultDistro[8]
            elif param_name == 'conv1_1_1x1_bn_sparse_running_var':
                param[i].lr_mult = lrMultDistro[9]
            else:
                param[i].lr_mult = lrMultDistro[10]
    
    # Set the batch size
    net.blobs['data'].reshape(batchSizes[0], 3, 227, 227)
    net.blobs['data_paf'].reshape(batchSizes[1], 2, 227, 227)
    net.blobs['data_heatmap'].reshape(batchSizes[2], numberOutputChannels, 227, 227)

    # set the solver
    solver = caffe.SGDSolver(os.path.join(caffeFolder, 'solver_' + layerName + '.prototxt'))
    solver.net.copy_from(os.path.join(caffeFolder, 'train_val_lmdb_' + layerName + '.caffemodel'))
    solver.net.params['conv1_1_1x1_bn'][0].data[...] = solver.net.params['conv1_1_1x1_bn'][0].data
    
    # train the network
    for i in range(numberIterations):
        solver.step(1)
        if i % 100 == 0:
            print('Iteration %d' % i)
            print('Saving model...')
            solver.net.save(os.path.join(trainedModelsFolder, 'model_' + layerName + '_iter_' + str(i) + '.caffemodel'))
            print('Saved model!')
            print('Saving solver state...')
            solver.net.save(os.path.join(trainedModelsFolder, 'solver_state_' + layerName + '_iter_' + str(i) + '.caffemodel'))
            print('Saved solver state!')
            print('Saving snapshot...')
            solver.snapshot()
            print('Saved snapshot!')
            print('Saving solver state...')
    
    # output the trained model
    solver.net.save(os.path.join(trainedModelsFolder, 'model_' + layerName + '.caffemodel'))

    
    


