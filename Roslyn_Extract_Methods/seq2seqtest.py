import string

import numpy as np
import keras
# import seq2seq
# from seq2seq.models import Seq2Seq
import seq2seq

import modelscopy
import operator
from keras.models import Sequential
from keras.layers.embeddings import Embedding
from keras.preprocessing import sequence


def main():
    max_eng_len = 20
    max_api_len = 10
    embedding_dim = 1200
    eng_top = 100
    eng_skip = 5
    api_top = 100
    api_skip = 0
    (eng, api) = get_data(eng_top, eng_skip, api_top, api_skip, False)
    eng = sequence.pad_sequences(eng, maxlen=max_eng_len)
    api = sequence.pad_sequences(api, maxlen=max_api_len)
    input_dim = max_eng_len
    hidden_dim = 10
    output_length = max_api_len
    output_dim = 8
    encoder_depth = 3
    decoder_depth = 3
    model = Sequential()

    eng_3d = []
    for sentence in eng:
        sent_new = []
        for word in sentence:
            sent_new.append([word, word])
        for word in sent_new:
            word.__delitem__(1)
        eng_3d += [sent_new]

    api_3d = []
    model = Sequential()
    for sentence in api:
        sent_new = []
        for word in sentence:
            sent_new.append([word,word])
        api_3d += [sent_new]
    eng_3d_np = np.array(eng_3d)
    api_3d_np = np.array(api_3d)
    a = 3
    #
    # model = SimpleSeq2Seq(input_dim=1, hidden_dim=hidden_dim, output_length=output_length, output_dim=1,
    #                   depth=(encoder_depth, decoder_depth))
    # model.compile(loss='mse', optimizer='rmsprop')
    # model.fit(eng_3d, api_3d, nb_epoch=3, batch_size=64)

    # model.add(Embedding(eng_top, embedding_dim, input_length=max_eng_len))
    model.add(Embedding(eng_top, embedding_dim, input_length=max_eng_len))
    model.add(
        modelscopy.AttentionSeq2Seq(output_length=max_api_len, output_dim=2, input_length=max_eng_len, input_dim=embedding_dim))
    # model.add(
    #     SimpleSeq2Seq(input_dim=input_dim, hidden_dim=hidden_dim, output_length=output_length, output_dim=output_dim,
    #                   depth=(encoder_depth, decoder_depth)))
    model.compile(loss='mse', optimizer='rmsprop')
    # model.fit(eng, api_3d_np, nb_epoch=5, batch_size=64)

def clean_up_list_of_lists(xs):
    def clean_up(words):
        for i, word in enumerate(words):
            words[i] = word.lower().translate(None, string.punctuation)

    for i, x in enumerate(xs):
        clean_up(x)


def read_from_file(filename="data.txt", ifcleanup=True):
    english_desc = []
    api_desc = []
    with open(filename, "r") as f:
        while True:
            line = f.readline().strip()
            if line == '':
                break
            if line.startswith("*") or line.startswith("/"):
                continue
            english_desc += [line.split(" ")]
            line = f.readline().strip()
            api_desc += [line.split(" ")]

    if ifcleanup:
        clean_up_list_of_lists(english_desc)
    # clean_up_list_of_lists(api_desc)
    return english_desc, api_desc


def construct_good_set(data, top=300, skip=10):
    word_cnt = {}
    for sentence in data:
        for word in sentence:
            if word_cnt.__contains__(word):
                word_cnt[word] += 1
            else:
                word_cnt[word] = 1
    top_eng_words = sorted(word_cnt.items(), key=operator.itemgetter(1), reverse=True)
    return set([key for (key, value) in top_eng_words][skip:top + skip - 1])


def filter_sentences(data, good_set):
    words_to_nums = {}
    cur = 1
    for i, sentence in enumerate(data):
        data[i] = filter(lambda x: x in good_set, sentence)
        for j, word in enumerate(data[i]):
            if not words_to_nums.__contains__(word):
                words_to_nums[word] = cur
                cur += 1
            data[i][j] = words_to_nums[word]
        # data[i] = filter(lambda x: x in good_set, sentence)
        # data[i] = map(lambda x: 1 if words_to_nums.__contains__(x) else 2, sentence)


def get_data(eng_top=10000, eng_skip=5, api_top=10000, api_skip=0, fromfile=False):
    if fromfile:
        return read_from_file("clean.txt", False)
    else:
        (eng, api) = read_from_file()
        eng_set = construct_good_set(eng, top=eng_top, skip=eng_skip)
        api_set = construct_good_set(api, top=api_top, skip=api_skip)
        filter_sentences(eng, eng_set)
        filter_sentences(api, api_set)
        return eng, api

def refactored_data_to_file():
    (eng, api) = get_data()
    filename = "clean.txt"
    with open(filename, 'w') as f:
        for words_e, words_a in zip(eng, api):
            if len(words_a) == 0:
                continue
            for word in words_e:
                f.write(str(word) + " ")
            f.write("\n")
            for word in words_a:
                f.write(str(word) + " ")
            f.write("\n")

# ls = ["sadlf32", "23r4,asdf", "sdfdsaf"]
# lb = ["asdf", "sdsaf,", "sad1\n"]
# ll = [ls, lb]
# clean_up_list_of_lists(ll)
# print ll

# print construct_good_set(200, 0)

# get_data()
#refactored_data_to_file()
main()