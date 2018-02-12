app.controller('PageCtrl', ['$scope','$rootScope','ajaxService','$window','$localStorage','$location', function($scope,$rootScope,ajaxService,$window,$localStorage,$location)
{
  
  $scope.loggedin=false;
  //alert(JSON.stringify(sessionStorage.getItem("SID")));
  if ((sessionStorage.getItem('SID')) > 0) {
      //alert($localStorage.UserObject["UserValidated"]);
      // var userobj = JSON.stringify(sessionStorage.getItem('UserObject'));
      // var obj=[];
      // var obj=sessionStorage.getItem('UserObject');
      // alert($.parseJSON(sessionStorage.getItem('UserObject')));
      // console.log($.parseJSON(sessionStorage.getItem('UserObject')).UserValidated);
      var isValidated=sessionStorage.getItem('UserObject').UserValidated;
      
      $scope.loggedin =isValidated;
      
      
        
    } 
    else 
    {
         
         $window.location.href='/AccessDenied.aspx';
    }
  
  

	
	
}]);