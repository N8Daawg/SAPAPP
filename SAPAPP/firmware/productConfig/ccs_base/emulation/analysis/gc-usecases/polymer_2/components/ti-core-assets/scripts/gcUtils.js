/***************************************************************************************************
 Copyright (c) 2017, Texas Instruments Incorporated
 All rights reserved.

 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions
 are met:

 *   Redistributions of source code must retain the above copyright
 notice, this list of conditions and the following disclaimer.
 notice, this list of conditions and the following disclaimer in the
 documentation and/or other materials provided with the distribution.
 *   Neither the name of Texas Instruments Incorporated nor the names of
 its contributors may be used to endorse or promote products derived
 from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 **************************************************************************************************/
var gc = gc || {};
gc.utils = gc.utils || {};

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

if (gc.utils.getValueFromHexString === undefined)
{
    (function() // closure for private static methods and data.
    {
        gc.utils.string2value = function(value)
        {
            if (typeof value === 'string' || value instanceof String)
            {
                value = value.trim();
                if (value.indexOf('"') === 0 || value.indexOf("'") === 0)
                {
                    // literal string - remove quotes
                    value = value.substring(1, value.length - 1);
                }
                else if (!isNaN(value))
                {
                    // numberic input
                    value = +value;
                }
                else if (value === 'true')
                {
                    value = true;
                }
                else if (value === 'false')
                {
                    value = false;
                }
            }
            return value;
        };

        gc.utils.value2string = function(value)
        {
            if (typeof value === 'object')
            {
                return JSON.stringify(value);
            }
            else
            {
                return "" + value;
            }
        };

        gc.utils.getValueFromHexString = function (hexString) {
            var result = hexString;
            if (!hexString.indexOf) {
                console.log("gc.utils.getValueFromHexString("+hexString+"): error, must pass in a string");
                return hexString;
            }
            try {
                var temp = hexString.trim().toLowerCase();
                if (temp.indexOf("0x") >= 0) {
                    temp = temp.substring(temp.indexOf("0x"));
                    result = parseInt(temp, 16);
                } else {
                    result = parseInt(temp, 10);
                }
            }
            catch(ex){
                console.log("gc.utils.getValueFromHexString("+hexString+"): ex="+ex);
            }
            return result;
        };
        gc.utils.getHexString = function(valueArg,wordSizeInBits,prefix) {
            var value = +valueArg;
            var padChar = "0";
            if (value < 0 ){
                // take 2's complement
                value = ~valueArg + 1;
                padChar = "F";
            }
            if ((!wordSizeInBits) || (isNaN(wordSizeInBits))){
                wordSizeInBits = 8;
            }
            var numHexChars = wordSizeInBits >> 2;
            if ((wordSizeInBits % 4) > 0){
                numHexChars++;
            }

            var valueStr = value.toString(16);
            while (valueStr.length < numHexChars) {
                valueStr = padChar+valueStr;
            }
            var prefixStr = "0x";
            if (prefix !== undefined){
                prefixStr = prefix;
            }
            valueStr = prefixStr+valueStr.toUpperCase();
            return valueStr;
        };
        gc.utils.getBinaryStr = function(intValue,numBits){
            var result = parseInt(intValue).toString(2);
            while (result.length < numBits){
                result = '0'+result;
            }
            return  "b"+result;
        };
        gc.utils.getBitValue = function(bitIndex,dataValue,wordSizeInBits){
            if ((bitIndex === undefined)||(bitIndex === null)){
                bitIndex = 0;
            }
            var result = (dataValue >> bitIndex) & 1;
            if (bitIndex >= wordSizeInBits){
                result = "";
            }
            return result.toString();
        };
        gc.utils.strPrefixSeparator = "__";
        /**
         * removes any characters up to and including the gc.utils.strPrefixSeparator
         * from the id string that is passed in
         *
         * @param id - the id string to have the prefix removed from
         * @return id string with prefix removed
         */
        gc.utils.removePrefixFromId = function(id){
            var result = id;
            var sep = gc.utils.strPrefixSeparator;
            var indexSeparator = id.indexOf(sep);
            if ((indexSeparator >= 0) && (id.length > indexSeparator + sep.length)) {
                result = id.substring(indexSeparator+sep.length);
            }
            return result;
        },
        /**
         * updates the ids in the html fragment that is passed in to use the specified idPrefix.
         *
         * @param idPrefix - a string to use as a unique prefix.  Set to "" or null to remove any existing prefixes from the ids.
         * @param gistSection - a part of the gist that contains either CSS style definitons or element tags
         * @param isStyleSection - true if gistSection contains CSS style definitions, false if it contains element tags
         * @return the updated gistSection
         */
        gc.utils.updateIdPrefixes = function(idPrefix,gistSection,isStyleSection){
            var separator = gc.utils.strPrefixSeparator;
            var hasIdPrefix = ((idPrefix) && (idPrefix.length > 0));
            if (isStyleSection) {
                // remove any existing prefixes from style element ids that have a prefix already specified (as indicated by the presence
                // of 2 underscores in a row)
                // See https://regex101.com/ for a useful tool to understand better how this works.
                //var regExp1 = new RegExp('(?!#'+idPrefix+')#',"g");
                var regExp1 = new RegExp('#.+__',"g");
                result = gistSection.trim().replace(regExp1, '#');
                if (hasIdPrefix) {
                    // add a prefix to all style element ids
                    var regExp3 = new RegExp('#', "g");
                    result = result.replace(regExp3, '#' + idPrefix + separator);
                }
            } else {
                // remove the prefix from any tag ids
                var regExp2 = new RegExp(' id=\".+__',"g");
                result = gistSection.trim().replace(regExp2,' id="');
                if (hasIdPrefix) {
                    // add a prefix to all tag ids
                    var regExp4 = new RegExp('id=\"',"g");
                    result = result.replace(regExp4,' id="'+idPrefix+separator);
                }
            }
            return result;
        }
    }());
}
