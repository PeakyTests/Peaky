var $ = require('jquery');
var React = require('react');
var ReactDOM = require('react-dom');
var htmlContent = require('../App/peaky.html');
var update = require('immutability-helper');
require("babel-polyfill");

var insertKnockoutBindingsIntoDom = function () {
    var div = document.createElement('div');
    div.className = 'app';
    div.innerHTML = htmlContent;
    document.body.appendChild(div);
}

var testResults = [];
var uniqueIds = 0;

var Sandwich = React.createClass({
    render: function () {
        return (
         <div className="Sandwich">
            <h1>Peaky!</h1>
            <div className="testsAndResults">
                <AvailableTests runTest={this.runTest} testResults={this.state.testResults} />
                <div className="results">
                    {
                        this.state.testResults.map((test, i) =>
                            <div key={i} className="result">
                                <h3 className={test.result.toLowerCase() }>{test.result} - {test.target} - {test.name}</h3>
                                <pre><code className="json">{test.raw}</code></pre>
                            </div>
                        )
                    }
                </div>
            </div>
         </div>);
    },

    getInitialState: function () {
        return {
            testResults: []
        };
    },

    runTest: function (test, sectionName) {

        var sandwich = this;

        var key = uniqueIds++;
        var newState = update(sandwich.state.testResults, {
            $push: [{
                result: 'Pending',
                name: getTestName(test.Url),
                url: test.Url,
                target: sectionName,
                key: key,
                raw: "{}"
            }]
        });
        sandwich.setState({ testResults: newState });

        $.ajax({
            url: test.Url,
            type: "POST",
            data: {},
            dataType: 'json',
            success: function (data) {
                var result = sandwich.state.testResults.find(t => t.key == key);
                result.result = 'Passed';
                result.raw = JSON.stringify(data.responseJSON || data.responseText || {}, null, 2).replace(/[\\]+r[\\]+n/g, "\n");
                sandwich.setState({ testResults: sandwich.state.testResults });
                //hljs.highlightBlock($('pre code').first()[0]);
            },
            error: function (data) {
                var result = sandwich.state.testResults.find(t => t.key == key);
                result.result = 'Failed';
                result.raw = JSON.stringify(data.responseJSON || data.responseText || {}, null, 2).replace(/[\\]+r[\\]+n/g, "\n");
                sandwich.setState({ testResults: sandwich.state.testResults });
                //hljs.highlightBlock($('pre code').first()[0]);
            },
        });
    }
})

var AvailableTests = React.createClass({
    loadTests: function () {
        var url = "/" + (location.pathname + location.search).substr(1);
        $.ajax({
            url: url,
            context: document.body,
            dataType: 'json',
            success: function (data) {
                this.setState({
                    data: groupTests(data.Tests),
                    id: 1
                });
            }.bind(this),
            error: function (xhr, status, err) {
                console.error('#GET Error', status, err.toString());
            }.bind(this)
        });
    },

    runAll: function(testGroup) {
        testGroup.Tests.forEach(t => this.props.runTest(t, testGroup.sectionName))
    },

    getInitialState: function () {
        return {
            id: 1,
            data: []
        };
    },

    componentDidMount: function () {
        this.loadTests();
    },

    render: function () {
        var currentTests = this;
        return (
            <div className="AvailableTests" key={0}>
                {
              this.state.data.map(function (testGroup, i) {
                  return <div key={i} data={i}>
                          <h2 className="sectionName">{testGroup.sectionName}</h2>
                      {
                                  testGroup.Tests.map(function (test, j) {
                                      return <div key={j} className="testandhistory" data={j}>
                                        <div className="test" onClick={currentTests.props.runTest.bind(null, test, testGroup.sectionName)}>
                                            <i className="fa fa-arrow-circle-right"></i>
                                            <div className="testName">{test.TestName}</div>
                                        </div>
                                        <div className="history">
                                            {
                                            currentTests.props.testResults.filter(i => i.url == test.Url).map((t,k) => {
                                                var icon = "fa fa-spinner fa-pulse fa-fw";
                                                if (t.result == "Passed") {
                                                    icon = "fa fa-check-circle-o"
                                                };
                                                if (t.result == "Failed") {
                                                    icon = "fa fa-times-circle-o"
                                                };
                                                return <i className={icon + ' ' + t.result.toLowerCase()} key={k} data={k} aria-hidden="true"></i>
                                            }
                                            )}
                                        </div>
                                      </div>
                                  })
                    }
                    <div onClick={currentTests.runAll.bind(currentTests, testGroup)}>Run All</div>
                  </div>
              })
                }
            </div>);
    },
});

ReactDOM.render(
  <Sandwich />,
  document.getElementById('container')
);

var getTestName = function (url) {
    return url.substr(url.lastIndexOf('/') + 1).replace(/_/g, " ");
}

var groupTests = function (tests) {
    var groupedTests = DataGrouper(tests, ["Environment", "Application"]);

    var mappedTests = groupedTests.map(entry => ({
        sectionName: (entry.key.Application + ' ' + entry.key.Environment).toUpperCase(),
        key: entry.key,
        Tests: entry.vals.map((test, i) => ({
            key: i,
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
