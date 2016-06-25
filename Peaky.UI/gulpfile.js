// include plug-ins
var gulp = require('gulp');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var gulpUtil = require('gulp-util');
var del = require('del');

var config = {
    //Include all js files but exclude any min.js files and the reference files as it has unexpected characters
    src: ['Scripts/**/*.js', '!Scripts/**/_references.js', '!Scripts/**/*.min.js']
}

//delete the output file(s)
gulp.task('clean', function () {
    //del is an async function and not a gulp plugin (just standard nodejs)
    //It returns a promise, so make sure you return that from this task function
    //  so gulp knows when the delete is complete
    return del(['App/vendors.js']);
});

// Combine and minify all files from the app folder
// This tasks depends on the clean task which means gulp will ensure that the 
// Clean task is completed before running the scripts task.
gulp.task('scripts', function () {

    return gulp.src(config.src)
      .pipe(uglify().on('error', gulpUtil.log))
      .pipe(concat('vendors.js'))
      .pipe(gulp.dest('App/'));
});

//Set a default tasks
gulp.task('default', ['scripts'], function () { });