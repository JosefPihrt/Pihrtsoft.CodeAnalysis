// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    internal static class CodeFixOptions
    {
        [CodeFixOption("<COMMA_SEPARATED_LIST_OF_IGNORED_XML_TAGS>")]
        public const string CS1591_MissingXmlCommentForPubliclyVisibleTypeOrMember_IgnoredTags = "roslynator.CS1591.ignored_tags";
    }
}
