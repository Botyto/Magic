require "CodeNode"

Class.PropertyNode = {
	__inherit = "CodeNode",

	value_type = false, --value type (node, string or `false`)
	default = false, --default value for this node
}

function PropertyNode:GeneratePageContent(writer)
	local page = { }

	table.insert(page, writer:GenerateSection("Syntax"))
	table.insert(page, writer:GenerateSnippet(self.variants[1], self.language, self.summary))

	if self.default then
		table.insert(page, writer:GenerateSection("Default"))
		table.insert(page, writer:GenerateSnippet(self.default, self.language, "Default value."))
	end

	return page
end
