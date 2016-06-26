
$(document).ready(function () {
    insertKnockoutBindingsIntoDom();

    var testResults = [];

    var viewModel = {
        TestGroups: ko.observableArray(),
        TestResults: ko.observableArray(),
        runTest: function (url) {
            $.ajax({
                url: url,
                type: "POST",
                data: {},
                dataType: 'json',
                success: function (data) {
                    testResults.unshift({
                        result: '[ Passed ]' + '[ ' + getTestNameFromUrl(url) + ' ] ',
                        raw: JSON.stringify(data.responseJSON || {})
                    });
                    viewModel.TestResults(testResults);
                },
                error: function (data) {
                    testResults.unshift({
                        result: '[ Failed ]' + '[ ' + getTestNameFromUrl(url) + ' ] ',
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

var groupTests = function (tests) {
    var groupedTests = DataGrouper(tests, ["Environment", "Application"]);

    var mappedTests = groupedTests.map(entry => ({
        sectionName: (entry.key.Application + ' ' + entry.key.Environment).toUpperCase(),
        key: entry.key, 
        Tests: entry.vals.map(test => ({
            TestName: getTestNameFromUrl(test.Url),
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
    div.className = 'peaky';
    div.innerHTML =
        '<ul class="testGroups" data-bind="template: { foreach: TestGroups }">' +
        '  <li><h2 class="sectionName" data-bind="text: sectionName"></h2>' +
        '    <ul data-bind="template: { foreach: Tests }">' +
        '      <li class="test" data-bind="click: function () {$root.runTest(Url)}"><i class="fa fa-arrow-circle-right"></i><span data-bind="text: TestName"></span></li>' +
        '    </ul>' +
        '  </li>' +
        '</ul>' +
        '<ul class="results" data-bind="template: { foreach: TestResults }">' +
        '  <li><span class="result" data-bind="text: result + raw"></span></li>' +
        '</ul>';
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


