//alert('i am out2');
var app = angular.module('testapp', ['ui.grid', 'angularBootstrapNavTree']);
//alert('i am out1');
app.controller("testcontroller", function ($scope, $http)
{
    alert('i am in');
   
        //alert('m in function');
        var url = 'http://localhost:59215/auth/get_data';
        var results = $http({
            method: 'GET',
            url: url
        });

        //debugger;
        results.then(function (values) {
           // console.log(values.data);
            $scope.myData = values.data;
           // $scope.status = values.data;
            //console.log(values);
        });
   
        



});
