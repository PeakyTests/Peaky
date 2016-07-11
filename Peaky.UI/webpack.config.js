var webpack = require('webpack');
var path = require('path');

var BUILD_DIR = path.resolve(__dirname, 'App');
var APP_DIR = path.resolve(__dirname, 'App');

var config = {
  entry: APP_DIR + '/peaky.jsx',
  output: {
    path: BUILD_DIR,
    filename: 'peaky.js'
  },
   module : {
    loaders : [
      {
        test : /\.jsx?/,
        include : APP_DIR,
        loader : 'babel'
      },
      { test: /\.html$/, loader: 'html' }
    ]
  }
};

module.exports = config;