/*
 * MinIO .NET Library for Amazon S3 Compatible Cloud Storage, (C) 2020 MinIO, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Xml.Serialization;

namespace Cleint.DataModel;

[Serializable]
public sealed class SelectCompressionType
{
    // Constants for compression types under select API.
    public static readonly SelectCompressionType NONE = new("NONE");
    public static readonly SelectCompressionType GZIP = new("GZIP");
    public static readonly SelectCompressionType BZIP = new("BZIP2");

    public SelectCompressionType()
    {
    }

    public SelectCompressionType(string value)
    {
        CompressionType = value;
    }

    [XmlText] public string CompressionType { get; set; }
}