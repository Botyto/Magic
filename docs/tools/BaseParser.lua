require "lib"
require "Class"
require "ClassNode"
require "MethodNode"
require "PropertyNode"

Class.BaseParser = {
}

function BaseParser:GetFileLines(file)
	local lines = { }
	for line in io.lines(file) do
		table.insert(lines, line)
	end
	return lines
end

function BaseParser:ParseFile(file)
	return { }
end

function BaseParser:SetupNode(node)
	if IsKindOf(node, "PageNode") then
		node.file_path = node.title .. ".md.html"
	end
end
