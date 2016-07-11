var $ = require('jquery');
var React = require('react');
var ReactDOM = require('react-dom');
var htmlContent = require('../App/peaky.html');

$(document).ready(function () {
    insertKnockoutBindingsIntoDom();
    var testResults = [];

    var viewModel = {
        TestGroups: ko.observableArray(),
        TestResults: ko.observableArray(),
        runTest: function (url, testSection) {

            $.ajax({
                url: url,
                type: "POST",
                data: {},
                dataType: 'json',
                success: function (data) {
                    testResults.unshift({
                        result: 'Passed',
                        name: getTestName(url),
                        target: testSection.sectionName,
                        raw: JSON.stringify(data.responseJSON || data.responseText || {}, null, 2).replace(/[\\]+r[\\]+n/g, "\n")
                    });
                    viewModel.TestResults(testResults);
                    hljs.highlightBlock($('pre code').first()[0]);
                },
                error: function (data) {
                    testResults.unshift({
                        result: 'Failed',
                        name: getTestName(url),
                        target: testSection.sectionName,
                        raw: JSON.stringify(data.responseJSON || data.responseText || {}, null, 2).replace(/[\\]+r[\\]+n/g, "\n")
                    });
                    viewModel.TestResults(testResults);
                    hljs.highlightBlock($('pre code').first()[0]);
                },
                
            });
        }
    };

    ko.applyBindings(viewModel);

    $.ajax({
        url: "/" + (location.pathname + location.search).substr(1),
        context: document.body,
        success: function (data) {
            console.log(data);

            var groupedTests = groupTests(data.Tests);

            viewModel.TestGroups(groupedTests);
        }
    });
});

var getTestName = function(url) {
    return url.substr(url.lastIndexOf('/') + 1).replace(/_/g, " ");
}

var groupTests = function (tests) {
    var groupedTests = DataGrouper(tests, ["Environment", "Application"]);

    var mappedTests = groupedTests.map(entry => ({
        sectionName: (entry.key.Application + ' ' + entry.key.Environment).toUpperCase(),
        key: entry.key, 
        Tests: entry.vals.map(test => ({
            TestName: getTestName(test.Url),
            Url: test.Url,
            Tags: test.Tags
        }))
    }));

    return mappedTests;
}

var getTestNameFromUrl = function (url) {
    return url.substr(url.lastIndexOf('/') + 1).replace(/_/g, " ");
}

var insertKnockoutBindingsIntoDom = function () {
    var div = document.createElement('div');
    div.className = 'app';
    div.innerHTML = htmlContent;
    document.body.appendChild(div);
}

var DataGrouper = (function () {
    var has = function (obj, target) {
        return _.any(obj, function (value) {
            return _.isEqual(value, target);
        });
    };

    var keys = function (data, names) {
        return _.reduce(data, function (memo, item) {
            var key = _.pick(item, names);
            if (!has(memo, key)) {
                memo.push(key);
            }
            return memo;
        }, []);
    };

    var group = function (data, names) {
        var stems = keys(data, names);
        return _.map(stems, function (stem) {
            return {
                key: stem,
                vals: _.map(_.where(data, stem), function (item) {
                    return _.omit(item, names);
                })
            };
        });
    };
    return group;
}());


