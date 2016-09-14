var $ = require('jquery');
var React = require('react');
var ReactDOM = require('react-dom');
var htmlContent = require('../App/peaky.html');
var update = require('immutability-helper');
require("babel-polyfill");

var uniqueIds = 0;

var Sandwich = React.createClass({
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

    render: function () {
        return (
         <div className="Sandwich">
            <div className="header">
                <h1>Peaky!</h1>
                <div className="controls">
                    <i className="fa fa-filter clickable" aria-hidden="true" title={JSON.stringify([new Set(flatten(flatten(this.state.data.map((testGroup) => testGroup.Tests)).map((test) => test.Tags)))])}></i>
                    <i className="fa fa-trash clickable" aria-hidden="true" onClick={this.clearTestResults}></i>
                </div>
            </div>
            <div className="testsAndResults">
                <AvailableTests runTest={this.runTest} scrollTo={this.scrollElementIntoViewIfNeeded} testResults={this.state.testResults} data={this.state.data} />
                <div className="results">
                    {
                        this.state.testResults.map((test, i) =>
                            <div key={i} className="result">
                                <div className="header">
                                    <h3 className={test.result.toLowerCase() }>
                                        <i className={getIcon(test.result)} aria-hidden="true"></i>
                                        {test.target} - {test.name}
                                    </h3>
                                    <i className="fa fa-files-o clickable" aria-hidden="true" title="Copy test result to clipboard"></i>
                                </div>
                                <pre><code className="json">{test.raw}</code></pre>
                            </div>
                        )
                    }
                </div>
            </div>
         </div>);
    },

    componentDidMount: function () {
        this.loadTests();
    },

    getInitialState: function () {
        return {
            testResults: [],
            data: []
        };
    },

    scrollElementIntoViewIfNeeded: function (domNode) {
        var containerDomNode = React.findDOMNode(this);
        // Determine if `domNode` fully fits inside `containerDomNode`.
        // If not, set the container's scrollTop appropriately.
    },

    clearTestResults: function () {
        this.setState({ testResults: [] });
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
                hljs.highlightBlock($('pre code').last()[0]);
            },
            error: function (data) {
                var result = sandwich.state.testResults.find(t => t.key == key);
                result.result = 'Failed';
                result.raw = JSON.stringify(data.responseJSON || data.responseText || {}, null, 2).replace(/[\\]+r[\\]+n/g, "\n");
                sandwich.setState({ testResults: sandwich.state.testResults });
                hljs.highlightBlock($('pre code').last()[0]);
            },
        });
    }
})

var AvailableTests = React.createClass({
    runAll: function (testGroup) {
        testGroup.Tests.forEach(t => this.props.runTest(t, testGroup.sectionName))
    },

    render: function () {
        var currentTests = this;
        return (
            <div className="AvailableTests" key={0}>
                {
              currentTests.props.data.map(function (testGroup, i) {
                  return <div key={i} data={i}>
                          <h2 className="sectionName">{testGroup.sectionName}</h2>
                      {
                                  testGroup.Tests.map(function (test, j) {
                                      return <div key={j} className="testandhistory" data={j}>
                                        <div className="test clickable" onClick={currentTests.props.runTest.bind(null, test, testGroup.sectionName)}>
                                            <i className="fa fa-arrow-circle-right"></i>
                                            <div className="testName">{test.TestName}</div>
                                        </div>
                                        <div className="history">
                                            {
                                            currentTests.props.testResults.filter(i => i.url == test.Url).map((t, k) => {
                                                var icon = getIcon(t.result);
                                                return <i className={icon} key={k} data={k} aria-hidden="true"></i>
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

var flatten = function flatten(arr) {
    return arr.reduce(function (flat, toFlatten) {
        return flat.concat(Array.isArray(toFlatten) ? flatten(toFlatten) : toFlatten);
    }, []);
}

var getIcon = function (result) {
    var icon = "fa fa-spinner fa-pulse fa-fw";
    if (result == "Passed") {
        icon = "fa fa-check-circle-o"
    };
    if (result == "Failed") {
        icon = "fa fa-times-circle-o"
    };
    return icon + ' ' + result.toLowerCase();
}

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
    })).sort(function (a, b) { return (a.sectionName > b.sectionName) ? 1 : ((b.sectionName > a.sectionName) ? -1 : 0); });

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
