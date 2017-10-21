(function (ng) {
    
    function bigingtoDate(bigint) {      
        var tempDate = /(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})/.exec(bigint.toString());                
        return new Date(tempDate[1], tempDate[2]-1, tempDate[3], tempDate[4], tempDate[5], tempDate[6],0);
    };

    angular.module('MyApp', ['smart-table', 'ui.bootstrap'])
   .controller('MainCtrl', ['$scope', '$http', function ($scope, $http) {
        $http.get('/home/GetComInit')
            .success(function (result) {
                $scope.rowCollection = result;
            }).error(function (data) {
                console.log(data);
            });
        $scope.info = function (jsonobject) {
            $scope.currentinfo = jsonobject;
            
            $scope.fpinfo = { id: jsonobject.id, FPNumber: jsonobject.FPNumber };
            $scope.fpinfo.Version = jsonobject.Version;
            $scope.fpinfo.DateTimeBegin = bigingtoDate(jsonobject.DateTimeBegin);
            $scope.fpinfo.DateTimeStop = bigingtoDate(jsonobject.DateTimeStop);
            $scope.fpinfo.DeltaTime = jsonobject.DeltaTime;
            $scope.fpinfo.CurrentSystemDateTime = new Date(parseInt(jsonobject.CurrentSystemDateTime.replace("/Date(", "").replace(")/", ""), 10));
            $scope.fpinfo.DateTimeSyncFP = new Date(parseInt(jsonobject.DateTimeSyncFP.replace("/Date(", "").replace(")/", ""), 10));
            
            $scope.fpinfo.DataServer = jsonobject.DataServer;
            $scope.fpinfo.DataBaseName = jsonobject.DataBaseName;

            $scope.fpinfo.MoxaIP = jsonobject.MoxaIP;
            $scope.fpinfo.MoxaPort = jsonobject.MoxaPort;
            
            $scope.fpinfo.Moxa = jsonobject.MoxaIP + ':' + jsonobject.MoxaPort;

            $scope.fpinfo.SmenaOpened= jsonobject.SmenaOpened;
            $scope.fpinfo.PapStat = jsonobject.PapStat;
            $scope.fpinfo.ByteStatusInfo = jsonobject.ByteStatusInfo;
            $scope.fpinfo.ByteResultInfo = jsonobject.ByteResultInfo;
            $scope.fpinfo.ByteReservInfo = jsonobject.ByteReservInfo;
            $scope.fpinfo.MinSumm = jsonobject.MinSumm;
            $scope.fpinfo.MaxSumm = jsonobject.MaxSumm;
            $scope.fpinfo.TypeEvery = jsonobject.TypeEvery;
            $scope.fpinfo.PrintEvery = jsonobject.PrintEvery;
            $scope.fpinfo.KlefMem = jsonobject.KlefMem;
        };

        $scope.setActive = function (val) {
            //$scope.user.is_active = val;
        };
        $scope.itemsByPage=15;
        $scope.displayedCollection = [];
   }])
    
    .directive('stSelectDistinct', [function () {
        return {
            restrict: 'E',
            require: '^stTable',
            scope: {
                collection: '=',
                predicate: '@',
                predicateExpression: '='
            },
            template: '<select ng-model="selectedOption" ng-change="optionChanged(selectedOption)" ng-options="opt for opt in distinctItems"></select>',
            link: function (scope, element, attr, table) {
                var getPredicate = function () {
                    var predicate = scope.predicate;
                    if (!predicate && scope.predicateExpression) {
                        predicate = scope.predicateExpression;
                    }
                    return predicate;
                };

                scope.$watch('collection', function (newValue) {
                    var predicate = getPredicate();

                    if (newValue) {
                        var temp = [];
                        scope.distinctItems = ['All'];

                        angular.forEach(scope.collection, function (item) {
                            var value = item[predicate];
                            if (typeof value === 'boolean') {
                                var value1 = value.toString();
                                if (value1 && value1.trim().length > 0 && temp.indexOf(value1) === -1) {
                                    temp.push(value1);
                                }
                            }
                            else {
                                if (value && value.trim().length > 0 && temp.indexOf(value) === -1) {
                                    temp.push(value);
                                }
                            }
                        });
                        temp.sort();

                        scope.distinctItems = scope.distinctItems.concat(temp);
                        scope.selectedOption = scope.distinctItems[0];
                        scope.optionChanged(scope.selectedOption);
                    }
                }, true);

                scope.optionChanged = function (selectedOption) {
                    var predicate = getPredicate();

                    var query = {};

                    query.distinct = selectedOption;

                    if (query.distinct === 'All') {
                        query.distinct = '';
                    }

                    table.search(query, predicate);
                };
            }
        };
    }])

    .filter('customFilter', ['$filter', function ($filter) {
        var filterFilter = $filter('filter');
        var standardComparator = function standardComparator(obj, text) {
            text = ('' + text).toLowerCase();
            return ('' + obj).toLowerCase().indexOf(text) > -1;
        };

        return function customFilter(array, expression) {
            function customComparator(actual, expected) {
                if (typeof actual === 'boolean')
                {
                    actual = actual.toString();
                }
                var isBeforeActivated = expected.before;
                var isAfterActivated = expected.after;
                var isLower = expected.lower;
                var isHigher = expected.higher;
                var higherLimit;
                var lowerLimit;
                var itemDate;
                var queryDate;

                if (ng.isObject(expected)) {
                    //exact match
                    if (expected.distinct) {
                        if (!actual || actual.toLowerCase() !== expected.distinct.toLowerCase()) {
                            return false;
                        }

                        return true;
                    }

                    //matchAny
                    if (expected.matchAny) {
                        if (expected.matchAny.all) {
                            return true;
                        }

                        if (!actual) {
                            return false;
                        }

                        for (var i = 0; i < expected.matchAny.items.length; i++) {
                            if (actual.toLowerCase() === expected.matchAny.items[i].toLowerCase()) {
                                return true;
                            }
                        }

                        return false;
                    }

                    //date range
                    if (expected.before || expected.after) {
                        try {
                            if (isBeforeActivated) {
                                higherLimit = expected.before;

                                itemDate = new Date(actual);
                                queryDate = new Date(higherLimit);

                                if (itemDate > queryDate) {
                                    return false;
                                }
                            }

                            if (isAfterActivated) {
                                lowerLimit = expected.after;


                                itemDate = new Date(actual);
                                queryDate = new Date(lowerLimit);

                                if (itemDate < queryDate) {
                                    return false;
                                }
                            }

                            return true;
                        } catch (e) {
                            return false;
                        }

                    } else if (isLower || isHigher) {
                        //number range
                        if (isLower) {
                            higherLimit = expected.lower;

                            if (actual > higherLimit) {
                                return false;
                            }
                        }

                        if (isHigher) {
                            lowerLimit = expected.higher;
                            if (actual < lowerLimit) {
                                return false;
                            }
                        }

                        return true;
                    }
                    //etc

                    return true;

                }
                return standardComparator(actual, expected);
            }

            var output = filterFilter(array, expression, customComparator);
            return output;
        };
    }]);

    
    })(angular);


