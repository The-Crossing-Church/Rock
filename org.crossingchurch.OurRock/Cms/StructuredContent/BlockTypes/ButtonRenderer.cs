// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Rock.Cms.StructuredContent;

namespace org.crossingchurch.OurRock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The List block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "button" )]
    public class ButtonRenderer : StructuredContentBlockRenderer<ButtonData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, ButtonData data )
        {
            if ( !String.IsNullOrEmpty( data.Html ) )
            {
                writer.WriteLine( $"{data.Html}" );
            }
            else
            {
                if ( !String.IsNullOrEmpty( data.Url ) && !String.IsNullOrEmpty( data.Text ) )
                {
                    var className = "btn btn-";
                    if ( !String.IsNullOrEmpty( data.Color ) && data.Color.ToLower() == "accent" )
                    {
                        className += "accent";
                    }
                    else
                    {
                        className += "primary";
                    }
                    writer.WriteLine( $"<a href=\"{data.Url}\" target=\"{data.Target}\" class=\"{className}\">{data.Text}</a>" );
                }
            }
        }
    }
}
