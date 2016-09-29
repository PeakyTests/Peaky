/// <binding BeforeBuild='default' />
var gulp = require('gulp');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var gulpUtil = require('gulp-util');
var del = require('del');
var minifyCSS = require('gulp-minify-css');

//-----------------------------------------------------
//Prepare Peaky.css 
//-----------------------------------------------------
var styleSheets = {
    src: [
        'Content/css/font-awesome.min.css',
        'Content/Highlight/magula.css',
        'Content/Peaky.css',
    ]
};

gulp.task('cleanStyleSheet', function () {
    return del(['App/peaky.css']);
});

gulp.task('preparePeakyCss', ['cleanStyleSheet'], function () {

    return gulp.src(styleSheets.src)
      .pipe(minifyCSS())
      .pipe(concat('peaky.css'))
      .pipe(gulp.dest('App/'));
});

gulp.task('preparePeakyCss', ['cleanStyleSheet'], function () {

    return gulp.src(styleSheets.src)
      .pipe(minifyCSS())
      .pipe(concat('peaky.css'))
      .pipe(gulp.dest('App/'));
});

//-----------------------------------------------------
//Prepare fonts
//-----------------------------------------------------

gulp.task('cleanFonts', function () {
    return del(['App/fonts/*']);
});
gulp.task('copyFonts', ['cleanFonts'], function () {
    return gulp.src(['Content/fonts/**/*'])
        .pipe(gulp.dest('fonts/'));
});

gulp.task('default', ['preparePeakyCss', 'copyFonts'], function () { });