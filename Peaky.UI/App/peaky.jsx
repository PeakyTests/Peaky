var $ = require('jquery');
var React = require('react');
var ReactDOM = require('react-dom');
var htmlContent = require('../App/peaky.html');

var insertKnockoutBindingsIntoDom = function () {
    var div = document.createElement('div');
    div.className = 'app';
    div.innerHTML = htmlContent;
    document.body.appendChild(div);
}

var testResults = [];

var Sandwich = React.createClass({
    render: function () {
        return (
         <div className="Sandwich">
            <h1>Peaky!</h1>
            <div className="testsAndResults">
                <AvailableTests runTest={this.runTest} />
                <div className="results">

                    <div className="result">
                        {
                            this.state.testResults.map((test) =>
                                <div>
                                    <div>{test.result} - {test.target} - {test.name}:</div>
                                    <span><pre><code className="json">{test.raw}</code></pre></span>
                                </div>
                            )
                        }
                    </div>
                </div>
            </div>
         </div>);
    },

    getInitialState: function () {
        return {
            testResults: []
        };
    },

    runTest: function (test) {

        var sandwich = this;
        $.ajax({
            url: test.Url,
            type: "POST",
            data: {},
            dataType: 'json',
            success: function (data) {
                testResults.unshift({
                    result: 'Passed',
                    name: getTestName(test.Url),
                    target: "TODO section name",
                    raw: JSON.stringify(data.responseJSON || data.responseText || {}, null, 2).replace(/[\\]+r[\\]+n/g, "\n")
                });
                sandwich.setState({ testResults: testResults });
                hljs.highlightBlock($('pre code').first()[0]);
            },
            error: function (data) {
                testResults.unshift({
                    result: 'Failed',
                    name: getTestName(test.Url),
                    target: "TODO section name",
                    raw: JSON.stringify(data.responseJSON || data.responseText || {}, null, 2).replace(/[\\]+r[\\]+n/g, "\n")
                });
                sandwich.setState({ testResults: testResults });
                hljs.highlightBlock($('pre code').first()[0]);
            },
        });
    }
})

var AvailableTests = React.createClass({
    // get game info
    loadTests: function () {
        $.ajax({
            url: "/" + (location.pathname + location.search).substr(1),
            context: document.body,
            dataType: 'json',
            success: function (data) {
                var groupedTests = groupTests(data.Tests);
                this.setState({ data: groupedTests });
            }.bind(this),
            error: function (xhr, status, err) {
                console.error('#GET Error', status, err.toString());
            }.bind(this)
        });
    },

    getInitialState: function () {
        return {
            data: []
        };
    },

    componentDidMount: function () {
        this.loadTests();
    },

    render: function () {
        var currentTests = this;
        return (
          <div className="tests">
              {
              this.state.data.map(function (testGroup) {
                  return  <div>
                          <h2 className="sectionName">{testGroup.sectionName}</h2>
                      {
                                  testGroup.Tests.map(function (test, i) {
                                      return <div className="test" onClick={currentTests.props.runTest.bind(null, test)}>
                                        <i className="fa fa-arrow-circle-right"></i>
                                        <div className="testName">{test.TestName}</div>
                                      </div>
                                  })
                      }
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
