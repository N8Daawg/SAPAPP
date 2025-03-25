/***************************************************************************************************
 * Copyright (c) 2016 Texas Instruments and others All rights reserved. This program and the
 * accompanying materials are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors: Brian Cruickshank - Initial API and implementation
 **************************************************************************************************/
var gc = gc || {};
gc.desktop = gc.desktop || {};

	if (window.parent != window)
	{
	    window.parent.gc = window.parent.gc || {};
	    window.parent.gc.app = gc;
	}

	if (window.parent.gc)
	{
	    // take the designer from the parent iframe, if available.
	    gc.designer = gc.designer || window.parent.gc.designer;
	}
	if (window.global && global.document && global.document.gc)
	{
	    // take the designer from the global node-webkit document if available
	    gc.designer = gc.designer || global.document.gc.designer;
	}



if (gc.desktop.isDesktop === undefined)
{
    (function() // closure for private static methods and data.
    {
        gc.desktop.isDesktop = function (is_gcDesigner) {
            /* when the files are served by a webserver, serverConfig is initialized by the server-config.js file */
            var desktop = false;
            var keepAlive = false;
            if (gc.serverConfig != null) {
                desktop = !gc.serverConfig.isOnline;
                keepAlive = gc.serverConfig.keepAliveMonitorEnabled;
            }

            /* if we are running in the desktop mode and not a CCS mode, keep the server running */
            if (gc.desktop.__serverPingHandler == null && desktop && keepAlive) {
                gc.desktop.__serverPingHandler = setInterval(function() {
                    var xmlhttp = new XMLHttpRequest();
                    xmlhttp.onreadystatechange = function () {};
                    xmlhttp.open("GET", "/api/keepalive", true);
                    xmlhttp.send();
                }, 30000);
            }

            return desktop;
        };

        gc.desktop.isMobileDevice = function () {
            var result = false;
            if ((navigator.app) || (navigator.device)) {
                result = true;
            }

            return result;
        };

        gc.desktop.isCCS = function() {
            var hostname = window.location.hostname;
            return !gc.desktop.isDesktop() && (hostname === 'localhost' || hostname === '127.0.0.1');
        };

        gc.desktop.isDebugModeSupported = function(){
            var result = false;
            return result;
        };
        gc.desktop.isInDebugMode = function(){
            debugger;
            if (!gc.desktop.isDesktop()) return false;
            if (!gc.desktop.isDebugModeSupported()) return false;
            var result = false;
            var gui = require('nw.gui');
            if (gui.App.manifest.window.toolbar) {
                result = true;
            }
            return result;
        };
        gc.desktop.openBrowserWindow = function(url) {
            window.open(url);
        };

        gc.desktop.closeWindow = function(win,quitApp){
            var _win = win || window;
            
            // shutdown the server
            if (quitApp && gc.serverConfig.keepAliveMonitorEnabled) {
                var xmlhttp = new XMLHttpRequest();
                xmlhttp.onreadystatechange = function () {};
                xmlhttp.open("GET", "/api/shutdown", true);
                xmlhttp.send();

                window.location = "about:blank";
                _win.close();
            } else {
                _win.close();
            }

        };
        gc.desktop.getPathToDesignerFolder = function(){
            return gc.designer.folderName;
        };
        gc.desktop.getPathToWorkspaceFolder = function(){
            return gc.designer.workspace.folderName;
        };
        gc.desktop.isAppPreview = function(){
            debugger;
            var result = false;
            var url = document.baseURI;
            if (gc.desktop.isDesktop()){
                if (url.indexOf('/workspace/') > 0){
                    result = true;
                }
            } else {
                if (url.indexOf('/preview/') > 0){
                    result = true;
                }
            }
            return result;
        },
        gc.desktop.getPathToProjectFolder = function(pathRelativeToWorkspaceFolder){
            return gc.designer.project.folderName;
        };
        gc.desktop.getPathToComponentsFolder = function(){
            return gc.designer != null ? "designer/components" : "components";
        };
        gc.desktop.openExplorer = function(folderPath) {
            var xmlhttp = new XMLHttpRequest();
            xmlhttp.onreadystatechange = function () {};
            xmlhttp.open("GET", "/api/openExplorer?path=" + folderPath, true);
            xmlhttp.send();
        };
        gc.desktop.getOS = function(){
            var os  = 'linux';
            if (navigator.appVersion.indexOf("Mac") != -1) {
                os = 'osx';
            } else if (navigator.appVersion.indexOf("Win")!= -1) {
                os = 'win';
            }
            return os;
        };
        gc.desktop.getPathToSupportedDevicesJson = function() {
            var jsonFilePath = "/components/ti-core-backplane/supported_devices.json";
            return jsonFilePath;
        };
        gc.desktop.getPathToTargetSetupJson = function() {
            var jsonFilePath = "/components/ti-core-backplane/target_setup.json";
            return jsonFilePath;
        };

    }());
}
