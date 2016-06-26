
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
                        result: 'Pass',
                        raw: JSON.stringify(data.responseJSON || {})
                    });
                    viewModel.TestResults(testResults);
                },
                error: function (data) {
                    testResults.unshift({
                        result: 'Fail',
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
        sectionName: entry.key.Application + ' ' + entry.key.Environment,
        key: entry.key, 
        Tests: entry.vals.map(test => ({
            TestName: test.Url.substr(test.Url.lastIndexOf('/') + 1),
            Url: test.Url,
            Tags: test.Tags
        }))
    }));

    return mappedTests;
}

var insertKnockoutBindingsIntoDom = function () {
    var div = document.createElement('div');
    div.className = 'row';
    div.innerHTML =
        '<ul data-bind="template: { foreach: TestGroups }">' +
        '  <li><span class="sectionName" data-bind="text: sectionName"></span>' +
        '    <ul data-bind="template: { foreach: Tests }">' +
        '      <li><button id="run" data-bind="click: function () {$root.runTest(Url)}">Run</button><span class="test" data-bind="text: TestName"></span></li>' +
        '    </ul>' +
        '  </li>' +
        '</ul>'+
        '<ul data-bind="template: { foreach: TestResults }">' +
        '  <li><span class="result" data-bind="text: result + raw"></span></li>' +
        '</ul>';;
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


