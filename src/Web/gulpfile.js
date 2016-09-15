/// <binding />
var gulp = require('gulp');
var util = require('gulp-util');
var plumber = require('gulp-plumber');
var sourcemaps = require('gulp-sourcemaps');
var minifyHtml = require('gulp-html-minifier');
var webpack = require('webpack');
var webpackStream = require('webpack-stream');
var cssNano = require('gulp-cssnano');
var connect = require('gulp-connect');
var open = require('gulp-open');
var exec = require('child_process').exec;
var path = require('path');

var debug = (process.env.NODE_ENV !== 'production');

var config = {
    paths: {
        allSrc: ['./src/**/*', './static/**/*'],
        html: './src/*.html',
        css: './src/*.css',
        js: './src/**/*.js',
        mainJs: './src/main.tsx',
        statics: './static/**/*',
        dist: '../../docs'
    }
};

var uglifyJs = new webpack.optimize.UglifyJsPlugin({ comments: /a^/ });
var defines = new webpack.DefinePlugin({ DEBUG: debug, 'process.env.NODE_ENV': debug ? '"development"' : '"production"' });

var webpackConfig = {
    entry: config.paths.mainJs,
    output: {
        filename: 'app.js'
    },
    resolve: {
        extensions: ['', '.webpack.js', '.web.js', '.js', '.ts', '.tsx']
    },
    module: {
        loaders: [
            {
                test: /\.tsx?$/,
                exclude: /node_modules/,
                loader: 'babel-loader!eslint-loader!ts-loader'
            }
        ]
    },
    plugins: debug ? [defines] : [defines, uglifyJs],
    devtool: debug ? '#cheap-module-eval-source-map' : undefined
};

gulp.task('css', function () {
    return gulp.src(config.paths.css)
        .pipe(plumber())
        .pipe(debug ? sourcemaps.init() : util.noop())
        .pipe(cssNano())
        .pipe(debug ? sourcemaps.write() : util.noop())
        .pipe(gulp.dest(config.paths.dist));
});

gulp.task('js', function () {
    return gulp.src(config.paths.js)
        .pipe(plumber())
        .pipe(webpackStream(webpackConfig, webpack))
        .pipe(gulp.dest(config.paths.dist));
});

gulp.task('html', function () {
    return gulp.src(config.paths.html)
        .pipe(plumber())
        .pipe(minifyHtml({
            removeComments: true, collapseWhitespace: true, lint: true, keepClosingSlash: true, minifyJS: true
        }))
        .pipe(gulp.dest(config.paths.dist));
});

gulp.task('statics', function () {
    return gulp.src(config.paths.statics)
        .pipe(gulp.dest(config.paths.dist));
});

gulp.task('build', ['html', 'css', 'js', 'statics']);

gulp.task('refresh', ['build'], function () {
    gulp.src('./*')
        .pipe(connect.reload());
});

gulp.task('watch', ['build'], function () {
    gulp.watch(config.paths.allSrc, ['refresh']);
});

gulp.task('serve', ['build'], function () {
    connect.server({
        root: '../../docs',
        livereload: true
    });
});

gulp.task('open', ['serve'], function () {
    return gulp.src(__filename).pipe(open({ uri: 'http://localhost:8080/' }));
});

gulp.task('default', debug ? ['watch', 'open'] : ['build']);
