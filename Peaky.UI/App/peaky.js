
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
                        raw: JSON.stringify(data.responseJSON || {})
                    });
                    viewModel.TestResults(testResults);
                },
                error: function (data) {
                    testResults.unshift({
                        result: 'Failed',
                        name: getTestName(url),
                        target: testSection.sectionName,
                        raw: JSON.stringify(data.responseJSON)
                    });
                    viewModel.TestResults(testResults);
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

var insertKnockoutBindingsIntoDom = function () {
    var div = document.createElement('div');
    div.className = 'app';
    div.innerHTML =
        '<h1>Peaky</h1>' +
        '<div class="testsAndResults">' +
        '<div class="tests">' +
        '<div data-bind="template: { foreach: TestGroups }">' +
        '  <h2 class="sectionName" data-bind="text: sectionName"></h2>' +
        '    <div data-bind="template: { foreach: Tests }">' +
        '      <div id="run" data-bind="click: function () {$root.runTest(Url, $parent)}"><div class="test" data-bind="text: TestName"></div></div>' +
        '    </div>' +
        '</div>'+
        '</div>' +
               '<div class="results">' +
        '<div data-bind="template: { foreach: TestResults }">' +
        '  <div class="result" data-bind="text:  result + \' - \' + target + \' - \' + name + \':\' + raw"></div>' +
        '</div>' +
        '</div>' +
        '</div>';
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


